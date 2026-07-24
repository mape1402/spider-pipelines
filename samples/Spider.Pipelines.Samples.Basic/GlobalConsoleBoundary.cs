namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Core;

    /// <summary>
    /// Provides a globally registered sample execution boundary.
    /// </summary>
    public sealed class GlobalConsoleBoundary : IPipelineExecutionBoundary<OrderRequest, OrderReceipt>
    {
        private readonly SampleEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalConsoleBoundary"/> class.
        /// </summary>
        /// <param name="log">The sample event log.</param>
        public GlobalConsoleBoundary(SampleEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: begin {context.Request.OrderId}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: complete {context.Response.ReceiptId}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write("global boundary: cancel");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write("global boundary: dispose");
            return ValueTask.CompletedTask;
        }
    }
}
