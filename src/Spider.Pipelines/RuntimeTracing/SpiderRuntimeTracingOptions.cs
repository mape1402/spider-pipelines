namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Configures Spider runtime tracing behavior.
    /// </summary>
    public sealed class SpiderRuntimeTracingOptions
    {
        /// <summary>
        /// Gets or sets the runtime trace verbosity.
        /// </summary>
        public SpiderTraceVerbosity Verbosity { get; set; } = SpiderTraceVerbosity.Normal;

        /// <summary>
        /// Gets or sets the trace queue capacity.
        /// </summary>
        public int QueueCapacity { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the backpressure behavior.
        /// </summary>
        public SpiderTraceBackpressure Backpressure { get; set; } = SpiderTraceBackpressure.DropNewest;
    }
}
