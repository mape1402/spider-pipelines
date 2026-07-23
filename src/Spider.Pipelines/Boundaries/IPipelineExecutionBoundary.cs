namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Defines a provider-agnostic boundary that wraps a full Spider pipeline execution.
    /// </summary>
    public interface IPipelineExecutionBoundary
    {
        /// <summary>
        /// Begins a boundary scope for the current pipeline execution.
        /// </summary>
        /// <param name="context">The Spider-owned execution metadata for the current pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A boundary scope that will be completed, faulted, cancelled, and disposed by Spider.</returns>
        ValueTask<IPipelineExecutionBoundaryScope> BeginAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken);
    }
}
