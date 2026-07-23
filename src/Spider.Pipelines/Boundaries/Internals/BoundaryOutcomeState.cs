namespace Spider.Pipelines.Boundaries.Internals
{
    /// <summary>
    /// Represents the terminal state of a boundary-wrapped pipeline execution.
    /// </summary>
    internal enum BoundaryOutcomeState
    {
        /// <summary>
        /// The pipeline has not reached a terminal state.
        /// </summary>
        Pending,

        /// <summary>
        /// The pipeline completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The pipeline ended through cooperative cancellation.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The pipeline faulted with an exception.
        /// </summary>
        Faulted
    }
}
