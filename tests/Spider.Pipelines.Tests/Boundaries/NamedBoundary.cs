using Spider.Pipelines.Boundaries;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Provides a named test boundary.
    /// </summary>
    public abstract class NamedBoundary : IPipelineExecutionBoundary
    {
        private readonly string _name;
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedBoundary"/> class.
        /// </summary>
        /// <param name="name">The boundary name.</param>
        /// <param name="log">The event log.</param>
        protected NamedBoundary(string name, BoundaryEventLog log)
        {
            _name = name;
            _log = log;
        }

        /// <inheritdoc/>
        public virtual ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:begin");
            _log.Activate();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:complete");
            _log.Deactivate();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:fault:{exception.GetType().Name}");
            _log.Deactivate();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:cancel");
            _log.Deactivate();
            return ValueTask.CompletedTask;
        }

    }
}
