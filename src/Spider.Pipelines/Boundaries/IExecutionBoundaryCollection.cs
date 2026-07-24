namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Configures execution boundaries that apply only to the current invocation.
    /// </summary>
    public interface IExecutionBoundaryCollection
    {
        /// <summary>
        /// Adds a DI-resolved execution boundary type to the current invocation.
        /// </summary>
        /// <typeparam name="TBoundary">The boundary implementation type to resolve from DI.</typeparam>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IPipelineExecutionBoundary;

        /// <summary>
        /// Adds a DI-resolved execution boundary type to the current invocation.
        /// </summary>
        /// <param name="boundaryType">The boundary implementation type to resolve from DI.</param>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection AddExecutionBoundary(Type boundaryType);
    }
}
