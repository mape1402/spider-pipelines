namespace Spider.Pipelines.Boundaries.Internals
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Collects request-only invocation boundaries and creates them from the active service provider.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class ExecutionBoundaryCollection<TRequest> : IExecutionBoundaryCollection<TRequest>
    {
        private readonly IList<Func<IServiceProvider, IBoundary<TRequest>>> _boundaryFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionBoundaryCollection{TRequest}"/> class.
        /// </summary>
        public ExecutionBoundaryCollection()
        {
            _boundaryFactories = new List<Func<IServiceProvider, IBoundary<TRequest>>>();
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection<TRequest> AddExecutionBoundary(IBoundary<TRequest> boundary)
        {
            if (boundary == null)
                throw new ArgumentNullException(nameof(boundary));

            _boundaryFactories.Add(_ => boundary);
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection<TRequest> AddExecutionBoundary(Action<IExecutionBoundaryConfiguration<TRequest>> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var boundary = new DelegateExecutionBoundary<TRequest>();
            configure(boundary);
            _boundaryFactories.Add(_ => boundary);
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection<TRequest> AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IBoundary<TRequest>
        {
            _boundaryFactories.Add(serviceProvider => serviceProvider.GetRequiredService<TBoundary>());
            return this;
        }

        /// <summary>
        /// Creates the invocation boundaries from the configured factories.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve DI-backed boundaries.</param>
        /// <returns>The boundaries configured for the current invocation.</returns>
        public IReadOnlyCollection<IBoundary<TRequest>> CreateExecutionBoundaries(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return _boundaryFactories.Select(factory => factory(serviceProvider)).ToArray();
        }
    }
}
