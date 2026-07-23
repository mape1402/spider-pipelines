namespace Spider.Pipelines.Boundaries.Internals
{
    /// <summary>
    /// Captures the terminal outcome of the boundary-wrapped pipeline core.
    /// </summary>
    internal sealed class BoundaryOutcome
    {
        private BoundaryOutcome(BoundaryOutcomeState state, Exception exception = null)
        {
            State = state;
            Exception = exception;
        }

        /// <summary>
        /// Gets the outcome state.
        /// </summary>
        public BoundaryOutcomeState State { get; }

        /// <summary>
        /// Gets the original pipeline exception when the outcome is faulted.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Creates an outcome for a pipeline that has not terminated.
        /// </summary>
        /// <returns>A pending boundary outcome.</returns>
        public static BoundaryOutcome Pending()
            => new BoundaryOutcome(BoundaryOutcomeState.Pending);

        /// <summary>
        /// Creates an outcome for a successful pipeline.
        /// </summary>
        /// <returns>A completed boundary outcome.</returns>
        public static BoundaryOutcome Completed()
            => new BoundaryOutcome(BoundaryOutcomeState.Completed);

        /// <summary>
        /// Creates an outcome for a cooperatively cancelled pipeline.
        /// </summary>
        /// <returns>A cancelled boundary outcome.</returns>
        public static BoundaryOutcome Cancelled()
            => new BoundaryOutcome(BoundaryOutcomeState.Cancelled);

        /// <summary>
        /// Creates an outcome for a faulted pipeline.
        /// </summary>
        /// <param name="exception">The original exception thrown by the pipeline.</param>
        /// <returns>A faulted boundary outcome.</returns>
        public static BoundaryOutcome Faulted(Exception exception)
            => new BoundaryOutcome(BoundaryOutcomeState.Faulted, exception ?? throw new ArgumentNullException(nameof(exception)));

        /// <summary>
        /// Throws the original exception when the outcome is faulted.
        /// </summary>
        public void ThrowIfFaulted()
        {
            if (Exception != null)
                throw Exception;
        }
    }
}
