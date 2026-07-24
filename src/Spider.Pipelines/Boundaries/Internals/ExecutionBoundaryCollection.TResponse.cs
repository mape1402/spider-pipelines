namespace Spider.Pipelines.Boundaries.Internals
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Collects request/response invocation boundaries and creates them from the active service provider.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class ExecutionBoundaryCollection<TRequest, TResponse> : IExecutionBoundaryCollection<TRequest, TResponse>
    {
        private readonly IList<Func<IServiceProvider, IBoundary<TRequest, TResponse>>> _boundaryFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionBoundaryCollection{TRequest, TResponse}"/> class.
        /// </summary>
        public ExecutionBoundaryCollection()
        {
            _boundaryFactories = new List<Func<IServiceProvider, IBoundary<TRequest, TResponse>>>();
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection<TRequest, TResponse> AddExecutionBoundary(Action<IExecutionBoundaryConfiguration<TRequest, TResponse>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var boundary = new DelegateExecutionBoundary<TRequest, TResponse>();
            configure(boundary);
            _boundaryFactories.Add(_ => boundary);
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection<TRequest, TResponse> AddExecutionBoundary(Type boundaryType)
        {
            if (boundaryType == null)
                throw new ArgumentNullException(nameof(boundaryType));

            _boundaryFactories.Add(serviceProvider => ExecutionBoundaryTypeResolver.Resolve<TRequest, TResponse>(serviceProvider, boundaryType));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection<TRequest, TResponse> AddBoundary(Type boundaryType)
            => AddExecutionBoundary(boundaryType);

        /// <inheritdoc/>
        public IExecutionBoundaryCollection<TRequest, TResponse> AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IBoundary<TRequest, TResponse>
        {
            _boundaryFactories.Add(serviceProvider => serviceProvider.GetRequiredService<TBoundary>());
            return this;
        }

        /// <summary>
        /// Creates the invocation boundaries from the configured factories.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve DI-backed boundaries.</param>
        /// <returns>The boundaries configured for the current invocation.</returns>
        public IReadOnlyCollection<IBoundary<TRequest, TResponse>> CreateExecutionBoundaries(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return _boundaryFactories.Select(factory => factory(serviceProvider)).ToArray();
        }
    }
}
