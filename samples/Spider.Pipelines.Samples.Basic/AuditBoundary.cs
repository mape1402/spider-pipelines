namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a sample audit execution boundary.
    /// </summary>
    public sealed class AuditBoundary : PipelineExecutionBoundary
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
        public override ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write($"audit: begin {context.RequestType.Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write("audit: complete");
            return ValueTask.CompletedTask;
        }

    }
}
