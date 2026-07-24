namespace Spider.Pipelines.Samples.Basic
{
    /// <summary>
    /// Provides sample business logic.
    /// </summary>
    public sealed class SampleOrderService
    {
        private readonly SampleEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleOrderService"/> class.
        /// </summary>
        /// <param name="log">The sample event log.</param>
        public SampleOrderService(SampleEventLog log)
        {
            _log = log;
        }

        /// <summary>
        /// Places an order and returns a receipt.
        /// </summary>
        /// <param name="request">The order request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The generated order receipt.</returns>
        public Task<OrderReceipt> PlaceOrderAsync(OrderRequest request, CancellationToken cancellationToken)
        {
            _log.Write($"handler: placing order {request.OrderId}");
            return Task.FromResult(new OrderReceipt($"RCPT-{request.OrderId}", request.Total));
        }
    }
}
