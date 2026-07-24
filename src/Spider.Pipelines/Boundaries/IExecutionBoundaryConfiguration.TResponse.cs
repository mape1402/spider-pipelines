namespace Spider.Pipelines.Boundaries
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Configures delegate callbacks for a request/response execution boundary.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IExecutionBoundaryConfiguration<TRequest, TResponse>
    {
        /// <summary>
        /// Configures the callback invoked when the boundary begins.
        /// </summary>
        /// <param name="handler">The begin callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest, TResponse> OnBegin(Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary completes successfully.
        /// </summary>
        /// <param name="handler">The complete callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest, TResponse> OnComplete(Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary faults.
        /// </summary>
        /// <param name="handler">The fault callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest, TResponse> OnFault(Func<IReadOnlyContext<TRequest, TResponse>, Exception, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary cancels.
        /// </summary>
        /// <param name="handler">The cancel callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest, TResponse> OnCancel(Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked after the terminal boundary operation has been attempted.
        /// </summary>
        /// <param name="handler">The dispose callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest, TResponse> OnDispose(Func<IReadOnlyContext<TRequest, TResponse>, ValueTask> handler);
    }
}
