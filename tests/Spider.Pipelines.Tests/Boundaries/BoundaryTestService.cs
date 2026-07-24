namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Provides service methods used by boundary tests.
    /// </summary>
    public sealed class BoundaryTestService
    {
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundaryTestService"/> class.
        /// </summary>
        /// <param name="log">The optional boundary event log.</param>
        public BoundaryTestService(BoundaryEventLog log = null)
        {
            _log = log;
        }

        /// <summary>
        /// Handles a request by returning its length.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The request length.</returns>
        public Task<int> HandleAsync(string request, CancellationToken cancellationToken)
            => Task.FromResult(request.Length);

        /// <summary>
        /// Handles a request and records whether the boundary is active.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The request length.</returns>
        public Task<int> HandleWithBoundaryCheckAsync(string request, CancellationToken cancellationToken)
        {
            _log.Add(_log.Active ? "handler-active" : "handler-inactive");
            return Task.FromResult(request.Length);
        }

        /// <summary>
        /// Throws an exception for boundary fault tests.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that never completes successfully.</returns>
        public Task<int> ThrowAsync(string request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Handler failed.");
    }
}
