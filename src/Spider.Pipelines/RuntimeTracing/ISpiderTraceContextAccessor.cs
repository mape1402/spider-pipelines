namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Provides access to the current async runtime trace context.
    /// </summary>
    public interface ISpiderTraceContextAccessor
    {
        /// <summary>
        /// Gets or sets the current runtime trace context.
        /// </summary>
        SpiderTraceContext Current { get; set; }
    }
}
