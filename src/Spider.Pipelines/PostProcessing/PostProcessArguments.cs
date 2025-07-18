namespace Spider.Pipelines.PostProcessing
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Represents the arguments passed to a post-processing step in the pipeline.
    /// </summary>
    public class PostProcessArguments
    {
        /// <summary>
        /// Gets or sets an optional reason or message explaining the outcome of the operation.
        /// Useful for logging or auditing.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the duration of the pipeline execution.
        /// Useful for performance monitoring or diagnostics.
        /// </summary>
        public TimeSpan? ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets an optional key-value collection to pass arbitrary metadata 
        /// to the post-processing steps.
        /// </summary>
        public ConcurrentDictionary<string, object> Metadata { get; set; } = new ConcurrentDictionary<string, object>();
    }
}
