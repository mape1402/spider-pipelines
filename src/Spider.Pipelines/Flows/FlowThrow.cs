namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Represents an early exception outcome for a composed flow.
    /// </summary>
    public sealed class FlowThrow
    {
        private readonly Func<Exception> _exceptionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowThrow"/> class.
        /// </summary>
        /// <param name="exceptionFactory">The exception factory to execute.</param>
        public FlowThrow(Func<Exception> exceptionFactory)
        {
            _exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
        }

        /// <summary>
        /// Creates the exception represented by this outcome.
        /// </summary>
        /// <returns>The exception to throw.</returns>
        public Exception CreateException()
            => _exceptionFactory();
    }
}
