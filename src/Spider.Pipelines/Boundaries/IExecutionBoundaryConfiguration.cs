namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Configures delegate callbacks for a provider-agnostic execution boundary.
    /// </summary>
    public interface IExecutionBoundaryConfiguration
    {
        /// <summary>
        /// Configures the callback invoked when the boundary begins.
        /// </summary>
        /// <param name="handler">The begin callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration OnBegin(Func<PipelineExecutionContext, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary completes successfully.
        /// </summary>
        /// <param name="handler">The complete callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration OnComplete(Func<PipelineExecutionContext, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary faults.
        /// </summary>
        /// <param name="handler">The fault callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration OnFault(Func<PipelineExecutionContext, Exception, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked when the boundary cancels.
        /// </summary>
        /// <param name="handler">The cancel callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration OnCancel(Func<PipelineExecutionContext, CancellationToken, ValueTask> handler);

        /// <summary>
        /// Configures the callback invoked after the terminal boundary operation has been attempted.
        /// </summary>
        /// <param name="handler">The dispose callback.</param>
        /// <returns>The current boundary configuration.</returns>
        IExecutionBoundaryConfiguration OnDispose(Func<PipelineExecutionContext, ValueTask> handler);
    }
}
