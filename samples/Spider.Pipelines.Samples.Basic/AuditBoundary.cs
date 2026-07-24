namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a sample audit execution boundary.
    /// </summary>
    public sealed class AuditBoundary : IPipelineExecutionBoundary
    {
        private readonly SampleEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditBoundary"/> class.
        /// </summary>
        /// <param name="log">The sample event log.</param>
        public AuditBoundary(SampleEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write($"audit: begin {context.RequestType.Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write("audit: complete");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Write($"audit: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write("audit: cancel");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext context)
        {
            _log.Write("audit: dispose");
            return ValueTask.CompletedTask;
        }
    }
}
