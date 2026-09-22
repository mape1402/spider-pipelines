namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Represents a runtime event emitted by Spider while a pipeline, boundary, or flow executes.
    /// </summary>
    public sealed class SpiderTraceEvent
    {
        /// <summary>
        /// Gets or sets the trace identifier.
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Gets or sets the span identifier.
        /// </summary>
        public string SpanId { get; set; }

        /// <summary>
        /// Gets or sets the parent span identifier.
        /// </summary>
        public string ParentSpanId { get; set; }

        /// <summary>
        /// Gets or sets the static architecture component identifier when one is known.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Gets or sets the component kind.
        /// </summary>
        public string ComponentKind { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the operation name.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the trace event kind.
        /// </summary>
        public SpiderTraceEventKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the trace event status.
        /// </summary>
        public SpiderTraceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp for the event.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the measured duration when the event closes a span.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets the input type name.
        /// </summary>
        public string InputType { get; set; }

        /// <summary>
        /// Gets or sets the output type name.
        /// </summary>
        public string OutputType { get; set; }

        /// <summary>
        /// Gets or sets the exception summary when the event faulted.
        /// </summary>
        public SpiderTraceException Exception { get; set; }

        /// <summary>
        /// Gets or sets the event tags.
        /// </summary>
        public IReadOnlyDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the event metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
