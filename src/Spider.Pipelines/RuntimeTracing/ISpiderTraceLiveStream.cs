namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Defines live runtime trace event streaming operations.
    /// </summary>
    public interface ISpiderTraceLiveStream
    {
        /// <summary>
        /// Publishes a runtime trace event to live subscribers.
        /// </summary>
        /// <param name="traceEvent">The trace event to publish.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask PublishAsync(
            SpiderTraceEvent traceEvent,
            CancellationToken cancellationToken);

        /// <summary>
        /// Subscribes to runtime trace events.
        /// </summary>
        /// <param name="query">The live stream query.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The live trace event stream.</returns>
        IAsyncEnumerable<SpiderTraceEvent> SubscribeAsync(
            SpiderTraceQuery query,
            CancellationToken cancellationToken);
    }
}
