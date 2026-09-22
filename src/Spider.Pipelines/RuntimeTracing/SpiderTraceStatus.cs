namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Describes the terminal or current status of a runtime trace item.
    /// </summary>
    public enum SpiderTraceStatus
    {
        /// <summary>
        /// The trace item is currently running.
        /// </summary>
        Running,

        /// <summary>
        /// The trace item completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The trace item completed with a fault.
        /// </summary>
        Faulted,

        /// <summary>
        /// The trace item was cancelled cooperatively.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The trace item was skipped.
        /// </summary>
        Skipped,

        /// <summary>
        /// The trace item or event was dropped under backpressure.
        /// </summary>
        Dropped
    }
}
