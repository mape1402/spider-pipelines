namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Defines a contract for execution plans that can resolve fluent-configured boundaries.
    /// </summary>
    internal interface IExecutionBoundaryPlan
    {
        /// <summary>
        /// Resolves the execution boundaries configured directly on the pipeline builder.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve boundary implementations.</param>
        /// <returns>The execution boundaries configured for this pipeline.</returns>
        IReadOnlyCollection<IPipelineExecutionBoundary> CreateExecutionBoundaries(IServiceProvider serviceProvider);
    }
}
