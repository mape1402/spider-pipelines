namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Represents a complete runtime trace execution.
    /// </summary>
    public sealed class SpiderTrace
    {
        /// <summary>
        /// Gets or sets the trace identifier.
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Gets or sets the optional correlation identifier.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the trace status.
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
        /// Gets or sets the trace duration.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets the trace events.
        /// </summary>
        public IReadOnlyList<SpiderTraceEvent> Events { get; set; } = Array.Empty<SpiderTraceEvent>();
    }
}
