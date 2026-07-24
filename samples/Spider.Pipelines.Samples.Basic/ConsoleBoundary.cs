namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a sample provider-agnostic execution boundary.
    /// </summary>
    public sealed class ConsoleBoundary : IPipelineExecutionBoundary
    {
        private readonly SampleEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleBoundary"/> class.
        /// </summary>
        /// <param name="log">The sample event log.</param>
        public ConsoleBoundary(SampleEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            _log.Write($"boundary: begin {context.RequestType.Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            _log.Write("boundary: complete");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(
            PipelineExecutionContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _log.Write($"boundary: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            _log.Write("boundary: cancel");
            return ValueTask.CompletedTask;
        }

    }
}
