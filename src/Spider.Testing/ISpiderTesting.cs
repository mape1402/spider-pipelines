namespace Spider.Testing
{
    /// <summary>
    /// Defines the testing surface used to execute requests through Spider test instrumentation.
    /// </summary>
    public interface ISpiderTesting
    {
        /// <summary>
        /// Gets the execution trace recorded by the latest test execution.
        /// </summary>
        ExecutionTrace Trace { get; }

        /// <summary>
        /// Executes a request through the discovered Spider testing pipeline.
        /// </summary>
        /// <param name="request">The request to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The handler result, or <c>null</c> when the handler is request-only.</returns>
        Task<object> ExecuteAsync(object request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a request through the discovered Spider testing pipeline and casts the result.
        /// </summary>
        /// <typeparam name="TResponse">The expected response type.</typeparam>
        /// <param name="request">The request to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The typed handler result.</returns>
        Task<TResponse> ExecuteAsync<TResponse>(object request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Simulates a failure inside the specified boundary type.
        /// </summary>
        /// <typeparam name="TBoundary">The boundary type to fail.</typeparam>
        /// <param name="exception">The exception to throw from the boundary.</param>
        void FailInside<TBoundary>(Exception exception);
    }
}
