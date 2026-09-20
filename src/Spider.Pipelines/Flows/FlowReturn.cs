namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Represents an early response outcome for a composed flow.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used to create the response.</typeparam>
    /// <typeparam name="TResponse">The response type produced by the flow.</typeparam>
    public sealed class FlowReturn<TCurrent, TResponse>
    {
        private readonly Func<TCurrent, CancellationToken, Task<TResponse>> _responseFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowReturn{TCurrent, TResponse}"/> class.
        /// </summary>
        /// <param name="responseFactory">The response factory to execute.</param>
        public FlowReturn(Func<TCurrent, TResponse> responseFactory)
        {
            if (responseFactory == null)
                throw new ArgumentNullException(nameof(responseFactory));

            _responseFactory = (current, _) => Task.FromResult(responseFactory(current));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowReturn{TCurrent, TResponse}"/> class.
        /// </summary>
        /// <param name="responseFactory">The asynchronous response factory to execute.</param>
        public FlowReturn(Func<TCurrent, CancellationToken, Task<TResponse>> responseFactory)
        {
            _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
        }

        /// <summary>
        /// Creates the early response.
        /// </summary>
        /// <param name="current">The current active value.</param>
        /// <param name="cancellationToken">The cancellation token for the execution.</param>
        /// <returns>The early response.</returns>
        public Task<TResponse> CreateResponseAsync(TCurrent current, CancellationToken cancellationToken)
            => _responseFactory(current, cancellationToken);
    }
}
