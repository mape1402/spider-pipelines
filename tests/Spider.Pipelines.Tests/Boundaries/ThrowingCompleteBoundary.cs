using Spider.Pipelines.Boundaries;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Provides a boundary that throws while completing.
    /// </summary>
    public sealed class ThrowingCompleteBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingCompleteBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public ThrowingCompleteBoundary(BoundaryEventLog log) : base("throw-complete", log) { }

        /// <inheritdoc/>
        public override ValueTask CompleteAsync(PipelineExecutionContext<string, int> context, CancellationToken cancellationToken)
        {
            base.CompleteAsync(context, cancellationToken);
            throw new InvalidOperationException("Complete failed.");
        }
    }
}
