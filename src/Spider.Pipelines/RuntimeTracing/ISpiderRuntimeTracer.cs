namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Defines the runtime tracing operations used internally by Spider.
    /// </summary>
    public interface ISpiderRuntimeTracer
    {
        /// <summary>
        /// Gets a value indicating whether runtime tracing is enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Starts a trace span.
        /// </summary>
        /// <param name="definition">The span definition.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The started span scope.</returns>
        ValueTask<SpiderTraceScope> StartSpanAsync(
            SpiderTraceSpanDefinition definition,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a standalone trace event.
        /// </summary>
        /// <param name="traceEvent">The trace event to add.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AddEventAsync(
            SpiderTraceEvent traceEvent,
            CancellationToken cancellationToken = default);
    }
}
