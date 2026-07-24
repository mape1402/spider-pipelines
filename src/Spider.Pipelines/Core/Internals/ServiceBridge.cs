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
        protected IPipelineBuilder _pipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBridge{TService}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        /// <param name="service">The service instance to bridge.</param>
        public ServiceBridge(IServiceProvider serviceProvider, TService service)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(service));
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc/>
        public TService Service { get; }

        /// <inheritdoc/>
        public IServiceBridge<TService, TRequest> Attach<TRequest>(Action<IPipelineBuilder<TRequest>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var builder = new PipelineBuilder<TRequest>(_serviceProvider);
            config(builder);

            _pipelineBuilder = builder;

            return new ServiceBridge<TService, TRequest>(_serviceProvider, Service, _pipelineBuilder);
        }

        /// <inheritdoc/>
        public IServiceBridge<TService, TRequest, TResponse> Attach<TRequest, TResponse>(Action<IPipelineBuilder<TRequest, TResponse>> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var builder = new PipelineBuilder<TRequest, TResponse>(_serviceProvider);
            config(builder);

            _pipelineBuilder = builder;

            return new ServiceBridge<TService, TRequest, TResponse>(_serviceProvider, Service, _pipelineBuilder);
        }

        /// <inheritdoc/>
        public Task ExecuteAsync<TRequest>(Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync(targetHandler, request, Array.Empty<IBoundary<TRequest>>(), cancellationToken);

        /// <inheritdoc/>
        public Task ExecuteAsync<TRequest>(
            Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest>> executionBoundaries,
            CancellationToken cancellationToken = default)
        {
            if (targetHandler == null)
                throw new ArgumentNullException(nameof(targetHandler));

            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var serviceMethod = targetHandler.Compile();
            var targetMethod = serviceMethod.Invoke(Service);

            var pipeline = _pipelineBuilder.Typed<TRequest>().Build(targetMethod);
            return pipeline.RunAsync(request, executionBoundaries, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<TResponse> ExecuteAsync<TRequest, TResponse>(Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync(targetHandler, request, Array.Empty<IBoundary<TRequest, TResponse>>(), cancellationToken);

        /// <inheritdoc/>
        public Task<TResponse> ExecuteAsync<TRequest, TResponse>(
            Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest, TResponse>> executionBoundaries,
            CancellationToken cancellationToken = default)
        {
            if (targetHandler == null)
                throw new ArgumentNullException(nameof(targetHandler));

            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var serviceMethod = targetHandler.Compile();
            var targetMethod = serviceMethod.Invoke(Service);

            var pipeline = _pipelineBuilder.Typed<TRequest, TResponse>().Build(targetMethod);
            return pipeline.RunAsync(request, executionBoundaries, cancellationToken);
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
        public ServiceBridge(IServiceProvider serviceProvider, TService service, IPipelineBuilder pipelineBuilder) : base(serviceProvider, service)
        {
            _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync<TRequest>(targetHandler, request, cancellationToken);

        /// <inheritdoc/>
        public Task ExecuteAsync(
            Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest>> executionBoundaries,
            CancellationToken cancellationToken = default)
            => ExecuteAsync<TRequest>(targetHandler, request, executionBoundaries, cancellationToken);
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
        public ServiceBridge(IServiceProvider serviceProvider, TService service, IPipelineBuilder pipelineBuilder) : base(serviceProvider, service)
        {
            _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        }

        /// <inheritdoc/>
        public Task<TResponse> ExecuteAsync(Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler, TRequest request, CancellationToken cancellationToken = default)
            => ExecuteAsync<TRequest, TResponse>(targetHandler, request, cancellationToken);

        /// <inheritdoc/>
        public Task<TResponse> ExecuteAsync(
            Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest, TResponse>> executionBoundaries,
            CancellationToken cancellationToken = default)
            => ExecuteAsync<TRequest, TResponse>(targetHandler, request, executionBoundaries, cancellationToken);
    }
}
