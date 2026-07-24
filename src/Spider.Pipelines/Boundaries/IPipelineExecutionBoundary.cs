namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Defines a provider-agnostic boundary that wraps a full Spider pipeline execution.
    /// </summary>
    public interface IPipelineExecutionBoundary
    {
        /// <summary>
        /// Begins the boundary for the current pipeline execution.
        /// </summary>
        /// <param name="context">The Spider-owned execution metadata for the current pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask BeginAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken);

        /// <summary>
        /// Completes the boundary after the wrapped pipeline succeeds.
        /// </summary>
        /// <param name="context">The Spider-owned execution metadata for the current pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask CompleteAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken);

        /// <summary>
        /// Faults the boundary after the wrapped pipeline throws.
        /// </summary>
        /// <param name="context">The Spider-owned execution metadata for the current pipeline.</param>
        /// <param name="exception">The original exception thrown by the wrapped pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask FaultAsync(
            PipelineExecutionContext context,
            Exception exception,
            CancellationToken cancellationToken);

        /// <summary>
        /// Cancels the boundary after the wrapped pipeline ends through cooperative cancellation.
        /// </summary>
        /// <param name="context">The Spider-owned execution metadata for the current pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask CancelAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken);
    }
}
