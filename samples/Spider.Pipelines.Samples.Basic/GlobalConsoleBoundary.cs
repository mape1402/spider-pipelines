namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a globally registered sample execution boundary.
    /// </summary>
    public sealed class GlobalConsoleBoundary : IPipelineExecutionBoundary
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
        public ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: begin {context.RequestType.Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write("global boundary: complete");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write("global boundary: cancel");
            return ValueTask.CompletedTask;
        }
    }
}
