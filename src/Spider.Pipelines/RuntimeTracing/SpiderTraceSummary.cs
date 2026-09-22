namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Represents a compact runtime trace summary for listing views.
    /// </summary>
    public sealed class SpiderTraceSummary
    {
        /// <summary>
        /// Gets or sets the trace identifier.
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Gets or sets the root component identifier.
        /// </summary>
        public string RootComponentId { get; set; }

        /// <summary>
        /// Gets or sets the root display name.
        /// </summary>
        public string RootDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the request type name.
        /// </summary>
        public string RequestType { get; set; }

        /// <summary>
        /// Gets or sets the response type name.
        /// </summary>
        public string ResponseType { get; set; }

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
        /// Gets or sets the number of stored events.
        /// </summary>
        public int EventCount { get; set; }

        /// <summary>
        /// Gets or sets the number of dropped events.
        /// </summary>
        public int DroppedEventCount { get; set; }
    }
}
