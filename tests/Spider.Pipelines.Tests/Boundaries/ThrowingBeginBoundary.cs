using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Provides a boundary that throws while beginning.
    /// </summary>
    public sealed class ThrowingBeginBoundary : IPipelineExecutionBoundary<string, int>
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
        public ValueTask BeginAsync(IReadOnlyContext<string, int> context, CancellationToken cancellationToken)
        {
            _log.Add("throw-begin:begin");
            throw new InvalidOperationException("Begin failed.");
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(IReadOnlyContext<string, int> context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask FaultAsync(IReadOnlyContext<string, int> context, Exception exception, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask CancelAsync(IReadOnlyContext<string, int> context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        /// <inheritdoc/>
        public ValueTask DisposeAsync(IReadOnlyContext<string, int> context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;
    }
}
