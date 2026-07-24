using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Provides a boundary that throws while faulting.
    /// </summary>
    public sealed class ThrowingFaultBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingFaultBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public ThrowingFaultBoundary(BoundaryEventLog log) : base("throw-fault", log) { }

        /// <inheritdoc/>
        public override ValueTask FaultAsync(IReadOnlyContext<string, int> context, Exception exception, CancellationToken cancellationToken)
        {
            base.FaultAsync(context, exception, cancellationToken);
            throw new InvalidOperationException("Fault failed.");
        }
    }
}
