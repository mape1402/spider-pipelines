namespace Spider.Pipelines.Boundaries
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Configures delegate callbacks for a request-only execution boundary.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IExecutionBoundaryConfiguration<TRequest>
    {
        /// <summary>
        /// Configures the callback invoked when the boundary begins.
        /// </summary>
        /// <param name="handler">The begin callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest> OnBegin(Func<IReadOnlyContext<TRequest>, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary completes successfully.
        /// </summary>
        /// <param name="handler">The complete callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest> OnComplete(Func<IReadOnlyContext<TRequest>, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary faults.
        /// </summary>
        /// <param name="handler">The fault callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest> OnFault(Func<IReadOnlyContext<TRequest>, Exception, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary cancels.
        /// </summary>
        /// <param name="handler">The cancel callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest> OnCancel(Func<IReadOnlyContext<TRequest>, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked after the terminal boundary operation has been attempted.
        /// </summary>
        /// <param name="handler">The dispose callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration<TRequest> OnDispose(Func<IReadOnlyContext<TRequest>, ValueTask> handler);
    }
}
