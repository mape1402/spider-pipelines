namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Defines a contract for request-only execution plans that can resolve fluent-configured boundaries.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal interface IExecutionBoundaryPlan<TRequest>
    {
        /// <summary>
        /// Resolves the execution boundaries configured directly on the pipeline builder.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve boundary implementations.</param>
        /// <returns>The execution boundaries configured for this pipeline.</returns>
        IReadOnlyCollection<IPipelineExecutionBoundary<TRequest>> CreateExecutionBoundaries(IServiceProvider serviceProvider);
    }

    /// <summary>
    /// Defines a contract for request/response execution plans that can resolve fluent-configured boundaries.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal interface IExecutionBoundaryPlan<TRequest, TResponse>
    {
        /// <summary>
        /// Resolves the execution boundaries configured directly on the pipeline builder.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve boundary implementations.</param>
        /// <returns>The execution boundaries configured for this pipeline.</returns>
        IReadOnlyCollection<IPipelineExecutionBoundary<TRequest, TResponse>> CreateExecutionBoundaries(IServiceProvider serviceProvider);
    }
}
