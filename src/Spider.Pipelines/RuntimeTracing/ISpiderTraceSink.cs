namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Defines an export target for runtime trace events.
    /// </summary>
    public interface ISpiderTraceSink
    {
        /// <summary>
        /// Writes a runtime trace event to the sink.
        /// </summary>
        /// <param name="traceEvent">The trace event to write.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask WriteAsync(
            SpiderTraceEvent traceEvent,
            CancellationToken cancellationToken);
    }
}
