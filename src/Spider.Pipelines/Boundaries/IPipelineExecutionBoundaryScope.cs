namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Defines a scope opened by an <see cref="IPipelineExecutionBoundary"/>.
    /// </summary>
    public interface IPipelineExecutionBoundaryScope : IAsyncDisposable
    {
        /// <summary>
        /// Completes the boundary after the wrapped pipeline succeeds.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask CompleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Faults the boundary after the wrapped pipeline throws.
        /// </summary>
        /// <param name="exception">The original exception thrown by the wrapped pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask FaultAsync(Exception exception, CancellationToken cancellationToken);

        /// <summary>
        /// Cancels the boundary after the wrapped pipeline ends through cooperative cancellation.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask CancelAsync(CancellationToken cancellationToken);
    }
}
