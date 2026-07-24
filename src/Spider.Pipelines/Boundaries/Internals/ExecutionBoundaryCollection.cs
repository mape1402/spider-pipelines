namespace Spider.Pipelines.Boundaries.Internals
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Collects invocation boundaries and creates them from the active service provider.
    /// </summary>
    internal sealed class ExecutionBoundaryCollection : IExecutionBoundaryCollection
    {
        private readonly IList<Func<IServiceProvider, IPipelineExecutionBoundary>> _boundaryFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionBoundaryCollection"/> class.
        /// </summary>
        public ExecutionBoundaryCollection()
        {
            _boundaryFactories = new List<Func<IServiceProvider, IPipelineExecutionBoundary>>();
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IPipelineExecutionBoundary
        {
            _boundaryFactories.Add(serviceProvider => serviceProvider.GetRequiredService<TBoundary>());
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryCollection AddExecutionBoundary(Type boundaryType)
        {
            if (boundaryType == null)
                throw new ArgumentNullException(nameof(boundaryType));

            if (!typeof(IPipelineExecutionBoundary).IsAssignableFrom(boundaryType))
                throw new InvalidOperationException($"Boundary type '{boundaryType.FullName}' must implement IPipelineExecutionBoundary.");

            _boundaryFactories.Add(serviceProvider => (IPipelineExecutionBoundary)serviceProvider.GetRequiredService(boundaryType));
            return this;
        }

        /// <summary>
        /// Creates the invocation boundaries from the configured factories.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve DI-backed boundaries.</param>
        /// <returns>The boundaries configured for the current invocation.</returns>
        public IReadOnlyCollection<IPipelineExecutionBoundary> CreateExecutionBoundaries(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return _boundaryFactories.Select(factory => factory(serviceProvider)).ToArray();
        }
    }
}
