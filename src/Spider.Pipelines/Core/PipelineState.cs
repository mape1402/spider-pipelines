namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Represents the execution stage of a pipeline step.
    /// </summary>
    public enum PipelineState
    {
        /// <summary>
        /// The step runs before the main operation (preprocessor).
        /// </summary>
        OnPreProcess,

        /// <summary>
        /// The step runs instead of the main operation (override).
        /// </summary>
        OnTargeting,

        /// <summary>
        /// The step runs after the main operation (postprocessor).
        /// </summary>
        OnPostProcess
    }
}
