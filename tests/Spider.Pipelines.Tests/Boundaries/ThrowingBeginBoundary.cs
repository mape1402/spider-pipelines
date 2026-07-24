using Spider.Pipelines.Boundaries;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Provides a boundary that throws while beginning.
    /// </summary>
    public sealed class ThrowingBeginBoundary : IBoundary<string, int>
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
        public ValueTask BeginAsync(PipelineExecutionContext<string, int> context, CancellationToken cancellationToken)
        {
            _log.Add("throw-begin:begin");
            throw new InvalidOperationException("Begin failed.");
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext<string, int> context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext<string, int> context, Exception exception, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext<string, int> context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext<string, int> context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;
    }
}
