namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Core;

    /// <summary>
    /// Provides a sample execution boundary passed directly to a single invocation.
    /// </summary>
    public sealed class InvocationConsoleBoundary : IPipelineExecutionBoundary<OrderRequest, OrderReceipt>
    {
        private readonly SampleEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationConsoleBoundary"/> class.
        /// </summary>
        /// <param name="log">The sample event log.</param>
        public InvocationConsoleBoundary(SampleEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: begin {context.Request.OrderId}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: complete {context.Response.ReceiptId}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write("invocation boundary: cancel");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync(IReadOnlyContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write("invocation boundary: dispose");
            return ValueTask.CompletedTask;
        }
    }
}
