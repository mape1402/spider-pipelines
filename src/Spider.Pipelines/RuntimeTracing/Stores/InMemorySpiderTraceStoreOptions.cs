namespace Spider.Pipelines.RuntimeTracing.Stores
{
    /// <summary>
    /// Configures the in-memory runtime trace store.
    /// </summary>
    public sealed class InMemorySpiderTraceStoreOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of traces retained in memory.
        /// </summary>
        public int MaxTraces { get; set; } = 500;

        /// <summary>
        /// Gets or sets the maximum number of events retained per trace.
        /// </summary>
        public int MaxEventsPerTrace { get; set; } = 2000;

        /// <summary>
        /// Gets or sets the amount of time traces are retained in memory.
        /// </summary>
        public TimeSpan TraceTtl { get; set; } = TimeSpan.FromMinutes(30);
    }
}
