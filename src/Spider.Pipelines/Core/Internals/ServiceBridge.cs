namespace Spider.Pipelines.Core.Internals
{
    using System.Linq.Expressions;
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a bridge for a service to pipeline execution and configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service being bridged.</typeparam>
    internal class ServiceBridge<TService> : IServiceBridge<TService>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IList<Action<IExecutionBoundaryCollection>> _executionBoundaryConfigurations;
        protected IPipelineBuilder _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBridge{TService}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        /// <param name="service">The service instance to bridge.</param>
        public ServiceBridge(IServiceProvider serviceProvider, TService service)
            : this(serviceProvider, service, Array.Empty<Action<IExecutionBoundaryCollection>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBridge{TService}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        /// <param name="service">The service instance to bridge.</param>
        /// <param name="executionBoundaryConfigurations">The bridge-level execution boundary configurations.</param>
        protected ServiceBridge(IServiceProvider serviceProvider, TService service, IEnumerable<Action<IExecutionBoundaryCollection>> executionBoundaryConfigurations)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            _executionBoundaryConfigurations = executionBoundaryConfigurations?.ToList() ?? throw new ArgumentNullException(nameof(executionBoundaryConfigurations));
        }

        /// <inheritdoc/>
        public TService Service { get; }

        /// <inheritdoc/>
        public IServiceBridge<TService> AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IPipelineExecutionBoundary
        {
            _executionBoundaryConfigurations.Add(execution => execution.AddExecutionBoundary<TBoundary>());
            return this;
        }

        /// <inheritdoc/>
        public IServiceBridge<TService> AddExecutionBoundary(Type boundaryType)
        {
            if (boundaryType == null)
                throw new ArgumentNullException(nameof(boundaryType));

            if (!typeof(IPipelineExecutionBoundary).IsAssignableFrom(boundaryType))
                throw new InvalidOperationException($"Boundary type '{boundaryType.FullName}' must implement IPipelineExecutionBoundary.");

            _executionBoundaryConfigurations.Add(execution => execution.AddExecutionBoundary(boundaryType));
            return this;
        }

        /// <inheritdoc/>
        public IServiceBridge<TService> AddExecutionBoundary(Action<IExecutionBoundaryConfiguration> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            _executionBoundaryConfigurations.Add(execution => execution.AddExecutionBoundary(configure));
            return this;
        }

        /// <inheritdoc/>
        public IServiceBridge<TService, TRequest> Attach<TRequest>(Action<IPipelineBuilder<TRequest>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var builder = new PipelineBuilder<TRequest>(_serviceProvider);
            config(builder);

            _pipelineBuilder = builder;

            return new ServiceBridge<TService, TRequest>(_serviceProvider, Service, _pipelineBuilder, _executionBoundaryConfigurations);
        }

        /// <inheritdoc/>
        public IServiceBridge<TService, TRequest, TResponse> Attach<TRequest, TResponse>(Action<IPipelineBuilder<TRequest, TResponse>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var builder = new PipelineBuilder<TRequest, TResponse>(_serviceProvider);
            config(builder);

            _pipelineBuilder = builder;

            return new ServiceBridge<TService, TRequest, TResponse>(_serviceProvider, Service, _pipelineBuilder, _executionBoundaryConfigurations);
        }

        /// <inheritdoc/>
        public Task ExecuteAsync<TRequest>(Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
        {
            if (targetHandler == null)
                throw new ArgumentNullException(nameof(targetHandler));

            var serviceMethod = targetHandler.Compile();
            var targetMethod = serviceMethod.Invoke(Service);

            var pipeline = _pipelineBuilder.Typed<TRequest>().Build(targetMethod);
            var configureExecution = CreateExecutionBoundaryConfiguration();
            return configureExecution == null
                ? pipeline.RunAsync(request, cancellationToken)
                : pipeline.RunAsync(request, configureExecution, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<TResponse> ExecuteAsync<TRequest, TResponse>(Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
        {
            if (targetHandler == null)
                throw new ArgumentNullException(nameof(targetHandler));

            var serviceMethod = targetHandler.Compile();
            var targetMethod = serviceMethod.Invoke(Service);

            var pipeline = _pipelineBuilder.Typed<TRequest, TResponse>().Build(targetMethod);
            var configureExecution = CreateExecutionBoundaryConfiguration();
            return configureExecution == null
                ? pipeline.RunAsync(request, cancellationToken)
                : pipeline.RunAsync(request, configureExecution, cancellationToken);
        }

        /// <summary>
        /// Creates execution boundary configuration from the bridge-level boundary configuration actions.
        /// </summary>
        /// <returns>The execution boundary configuration, or <c>null</c> when none were configured.</returns>
        private Action<IExecutionBoundaryCollection> CreateExecutionBoundaryConfiguration()
        {
            if (_executionBoundaryConfigurations.Count == 0)
                return null;

            var boundaryConfigurations = _executionBoundaryConfigurations.ToArray();
            return execution =>
            {
                foreach (var configureBoundary in boundaryConfigurations)
                    configureBoundary(execution);
            };
        }
    }

    /// <summary>
    /// Provides a bridge for a service to pipeline execution for a specific request type.
    /// </summary>
    /// <typeparam name="TService">The type of the service being bridged.</typeparam>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal class ServiceBridge<TService, TRequest> : ServiceBridge<TService>, IServiceBridge<TService, TRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBridge{TService, TRequest}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        /// <param name="service">The service instance to bridge.</param>
        /// <param name="pipelineBuilder">The pipeline builder instance.</param>
        /// <param name="executionBoundaryConfigurations">The bridge-level execution boundary configurations.</param>
        public ServiceBridge(IServiceProvider serviceProvider, TService service, IPipelineBuilder pipelineBuilder, IEnumerable<Action<IExecutionBoundaryCollection>> executionBoundaryConfigurations) : base(serviceProvider, service, executionBoundaryConfigurations)
        {
            _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync<TRequest>(targetHandler, request, cancellationToken);

    }

    /// <summary>
    /// Provides a bridge for a service to pipeline execution for a specific request and response type.
    /// </summary>
    /// <typeparam name="TService">The type of the service being bridged.</typeparam>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal class ServiceBridge<TService, TRequest, TResponse> : ServiceBridge<TService>, IServiceBridge<TService, TRequest, TResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBridge{TService, TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        /// <param name="service">The service instance to bridge.</param>
        /// <param name="pipelineBuilder">The pipeline builder instance.</param>
        /// <param name="executionBoundaryConfigurations">The bridge-level execution boundary configurations.</param>
        public ServiceBridge(IServiceProvider serviceProvider, TService service, IPipelineBuilder pipelineBuilder, IEnumerable<Action<IExecutionBoundaryCollection>> executionBoundaryConfigurations) : base(serviceProvider, service, executionBoundaryConfigurations)
        {
            _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        }

        /// <inheritdoc/>
        public Task<TResponse> ExecuteAsync(Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync<TRequest, TResponse>(targetHandler, request, cancellationToken);

    }
}
