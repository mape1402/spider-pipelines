namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a sample execution boundary passed directly to a single invocation.
    /// </summary>
    public sealed class InvocationConsoleBoundary : IBoundary<OrderRequest, OrderReceipt>
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
        public ValueTask BeginAsync(PipelineExecutionContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: begin {context.Request.OrderId}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: complete {context.Response.ReceiptId}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext<OrderRequest, OrderReceipt> context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write("invocation boundary: cancel");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext<OrderRequest, OrderReceipt> context, CancellationToken cancellationToken)
        {
            _log.Write("invocation boundary: dispose");
            return ValueTask.CompletedTask;
        }
    }
}
