namespace Spider.Pipelines.Targeting
{
    /// <summary>
    /// Represents the arguments passed to an override condition delegate in the pipeline.
    /// </summary>
    public class OverridesConditionArguments
    {
        /// <summary>
        /// Gets or sets an optional reason or explanation for why the override condition was evaluated or triggered.
        /// This can be used for logging, debugging, or auditing purposes.
        /// </summary>
        public string Reason { get; set; }
    }
}