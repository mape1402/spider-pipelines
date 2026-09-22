namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Describes a span that Spider is about to start.
    /// </summary>
    public sealed class SpiderTraceSpanDefinition
    {
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
        /// Gets or sets the input type.
        /// </summary>
        public Type InputType { get; set; }

        /// <summary>
        /// Gets or sets the output type.
        /// </summary>
        public Type OutputType { get; set; }

        /// <summary>
        /// Gets or sets the event kind emitted when the span starts.
        /// </summary>
        public SpiderTraceEventKind StartedKind { get; set; } = SpiderTraceEventKind.TraceStarted;

        /// <summary>
        /// Gets or sets the event kind emitted when the span completes successfully.
        /// </summary>
        public SpiderTraceEventKind CompletedKind { get; set; } = SpiderTraceEventKind.TraceCompleted;

        /// <summary>
        /// Gets or sets the event kind emitted when the span faults.
        /// </summary>
        public SpiderTraceEventKind FaultedKind { get; set; } = SpiderTraceEventKind.TraceFaulted;

        /// <summary>
        /// Gets or sets the event kind emitted when the span is cancelled.
        /// </summary>
        public SpiderTraceEventKind CancelledKind { get; set; } = SpiderTraceEventKind.TraceCancelled;

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
