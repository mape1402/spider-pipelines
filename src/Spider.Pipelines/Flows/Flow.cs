namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Provides factory methods for explicit early-exit outcomes in composed flows.
    /// </summary>
    public static class Flow
    {
        /// <summary>
        /// Creates an early-return outcome for a flow with a response contract.
        /// </summary>
        /// <typeparam name="TCurrent">The active value type used to create the response.</typeparam>
        /// <typeparam name="TResponse">The response type produced by the flow.</typeparam>
        /// <param name="responseFactory">The response factory to execute when the condition is false.</param>
        /// <returns>An early-return outcome.</returns>
        public static FlowReturn<TCurrent, TResponse> Return<TCurrent, TResponse>(
            Func<TCurrent, TResponse> responseFactory)
            => new(responseFactory);

        /// <summary>
        /// Creates an early-return outcome for a flow with a response contract.
        /// </summary>
        /// <typeparam name="TCurrent">The active value type used to create the response.</typeparam>
        /// <typeparam name="TResponse">The response type produced by the flow.</typeparam>
        /// <param name="responseFactory">The asynchronous response factory to execute when the condition is false.</param>
        /// <returns>An early-return outcome.</returns>
        public static FlowReturn<TCurrent, TResponse> Return<TCurrent, TResponse>(
            Func<TCurrent, CancellationToken, Task<TResponse>> responseFactory)
            => new(responseFactory);

        /// <summary>
        /// Creates an early-fault outcome.
        /// </summary>
        /// <param name="exceptionFactory">The exception factory to execute when the condition is false.</param>
        /// <returns>An early-fault outcome.</returns>
        public static FlowThrow Throw(Func<Exception> exceptionFactory)
            => new(exceptionFactory);

        /// <summary>
        /// Creates an early-stop outcome for a flow without a response contract.
        /// </summary>
        /// <returns>An early-stop outcome.</returns>
        public static FlowStop Stop()
            => FlowStop.Instance;
    }
}
