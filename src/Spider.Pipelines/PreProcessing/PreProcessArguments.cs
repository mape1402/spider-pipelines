namespace Spider.Pipelines.PreProcessing
{
    /// <summary>
    /// Represents the arguments provided to a preprocessor step in the pipeline.
    /// </summary>
    public class PreProcessArguments
    {
        /// <summary>
        /// Gets or sets a value indicating whether the pipeline execution should be cancelled.
        /// </summary>
        public bool Cancelled { get; set; }
    }
}
