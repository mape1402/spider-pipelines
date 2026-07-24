using Spider.Pipelines.Boundaries;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Provides a boundary that throws while beginning.
    /// </summary>
    public sealed class ThrowingBeginBoundary : IPipelineExecutionBoundary
    {
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingBeginBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public ThrowingBeginBoundary(BoundaryEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Add("throw-begin:begin");
            throw new InvalidOperationException("Begin failed.");
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

    }
}
