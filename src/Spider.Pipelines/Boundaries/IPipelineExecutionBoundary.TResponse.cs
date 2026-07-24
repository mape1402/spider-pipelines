namespace Spider.Pipelines.Boundaries
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Defines a typed boundary that wraps a full request/response Spider pipeline execution.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IPipelineExecutionBoundary<TRequest, TResponse>
    {
        /// <summary>
        /// Begins the boundary for the current pipeline execution.
        /// </summary>
        /// <param name="context">The typed pipeline execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask BeginAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);

        /// <summary>
        /// Completes the boundary after the wrapped pipeline succeeds.
        /// </summary>
        /// <param name="context">The typed pipeline execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask CompleteAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);

        /// <summary>
        /// Faults the boundary after the wrapped pipeline throws.
        /// </summary>
        /// <param name="context">The typed pipeline execution context.</param>
        /// <param name="exception">The original exception thrown by the wrapped pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask FaultAsync(IReadOnlyContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken);

        /// <summary>
        /// Cancels the boundary after the wrapped pipeline ends through cooperative cancellation.
        /// </summary>
        /// <param name="context">The typed pipeline execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask CancelAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);

        /// <summary>
        /// Disposes boundary state after the terminal boundary operation has been attempted.
        /// </summary>
        /// <param name="context">The typed pipeline execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task-like value representing the asynchronous operation.</returns>
        ValueTask DisposeAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);
    }
}
