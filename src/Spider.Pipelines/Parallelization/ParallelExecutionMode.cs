namespace Spider.Pipelines.Parallelization
{
    /// <summary>
    /// Defines when configured parallel steps run relative to the target handler.
    /// </summary>
    public enum ParallelExecutionMode
    {
        /// <summary>
        /// Parallel steps run before the target handler.
        /// </summary>
        BeforeTarget,

        /// <summary>
        /// Parallel steps run at the same time as the target handler.
        /// </summary>
        WithTarget,

        /// <summary>
        /// Parallel steps run after the target handler.
        /// </summary>
        AfterTarget
    }
}
