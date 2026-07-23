namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Represents the state of an operation result.
    /// </summary>
    public enum ResultState
    {
        /// <summary>
        /// The operation has not completed yet and is still pending.
        /// </summary>
        Pending,

        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The operation failed to complete successfully.
        /// </summary>
        Failure,

        /// <summary>
        /// The operation was cancelled before completion.
        /// </summary>
        Cancelled
    }
}
