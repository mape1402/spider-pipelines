namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Represents a runtime span tracked by Spider.
    /// </summary>
    public sealed class SpiderTraceSpan
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
        /// Gets or sets the span status.
        /// </summary>
        public SpiderTraceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the UTC start timestamp.
        /// </summary>
        public DateTimeOffset StartedAt { get; set; }

        /// <summary>
        /// Gets or sets the UTC completion timestamp.
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the measured duration.
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
        /// Gets or sets the exception summary when the span faulted.
        /// </summary>
        public SpiderTraceException Exception { get; set; }

        /// <summary>
        /// Gets or sets the span tags.
        /// </summary>
        public IReadOnlyDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the span metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
