namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Controls how Spider handles trace events when the runtime tracing queue is full.
    /// </summary>
    public enum SpiderTraceBackpressure
    {
        /// <summary>
        /// Drops the newest event when the queue is full.
        /// </summary>
        DropNewest,

        /// <summary>
        /// Drops the oldest buffered event when the queue is full.
        /// </summary>
        DropOldest,

        /// <summary>
        /// Samples events when the queue is full.
        /// </summary>
        Sample,

        /// <summary>
        /// Blocks the writer until queue capacity is available.
        /// </summary>
        Block
    }
}
