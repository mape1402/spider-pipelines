namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Defines write operations for runtime traces.
    /// </summary>
    public interface ISpiderTraceWriter
    {
        /// <summary>
        /// Appends a runtime trace event.
        /// </summary>
        /// <param name="traceEvent">The trace event to append.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AppendAsync(
            SpiderTraceEvent traceEvent,
            CancellationToken cancellationToken);
    }
}
