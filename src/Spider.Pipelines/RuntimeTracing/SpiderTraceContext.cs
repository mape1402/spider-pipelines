namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Stores the current runtime trace span context.
    /// </summary>
    public sealed class SpiderTraceContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderTraceContext"/> class.
        /// </summary>
        /// <param name="traceId">The trace identifier.</param>
        /// <param name="spanId">The span identifier.</param>
        /// <param name="parentSpanId">The parent span identifier.</param>
        /// <param name="previous">The previous async trace context.</param>
        public SpiderTraceContext(
            string traceId,
            string spanId,
            string parentSpanId,
            SpiderTraceContext previous)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentSpanId;
            Previous = previous;
        }

        /// <summary>
        /// Gets the trace identifier.
        /// </summary>
        public string TraceId { get; }

        /// <summary>
        /// Gets the span identifier.
        /// </summary>
        public string SpanId { get; }

        /// <summary>
        /// Gets the parent span identifier.
        /// </summary>
        public string ParentSpanId { get; }

        /// <summary>
        /// Gets the previous async trace context.
        /// </summary>
        public SpiderTraceContext Previous { get; }
    }
}
