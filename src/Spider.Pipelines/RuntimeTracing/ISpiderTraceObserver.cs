namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Defines span lifecycle hooks for runtime tracing integrations.
    /// </summary>
    public interface ISpiderTraceObserver
    {
        /// <summary>
        /// Called when a runtime trace span starts.
        /// </summary>
        /// <param name="span">The started span.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask OnSpanStartedAsync(
            SpiderTraceSpan span,
            CancellationToken cancellationToken);

        /// <summary>
        /// Called when a runtime trace span completes.
        /// </summary>
        /// <param name="span">The completed span.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask OnSpanCompletedAsync(
            SpiderTraceSpan span,
            CancellationToken cancellationToken);
    }
}
