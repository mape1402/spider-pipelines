namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Configures execution boundaries that apply to the current materialized execution.
    /// </summary>
    public interface IExecutionBoundaryCollection
    {
        /// <summary>
        /// Adds a DI-resolved execution boundary type to the current materialized execution.
        /// </summary>
        /// <typeparam name="TBoundary">The boundary implementation type to resolve from DI.</typeparam>
        /// <returns>The current execution boundary collection.</returns>
        IExecutionBoundaryCollection AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IPipelineExecutionBoundary;

        /// <summary>
        /// Adds a DI-resolved execution boundary type to the current materialized execution.
        /// </summary>
        /// <param name="boundaryType">The boundary implementation type to resolve from DI.</param>
        /// <returns>The current execution boundary collection.</returns>
        IExecutionBoundaryCollection AddExecutionBoundary(Type boundaryType);
    }
}
