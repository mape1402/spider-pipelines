namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Describes a runtime trace query.
    /// </summary>
    public sealed class SpiderTraceQuery
    {
        /// <summary>
        /// Gets or sets the optional status filter.
        /// </summary>
        public SpiderTraceStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the optional search text.
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of traces to return.
        /// </summary>
        public int Limit { get; set; } = 100;
    }
}
