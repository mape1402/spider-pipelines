using Spider.Pipelines.Boundaries;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Records lifecycle events for an open-generic request/response execution boundary.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public sealed class OpenRecordingBoundary<TRequest, TResponse> : IBoundary<TRequest, TResponse>
    {
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenRecordingBoundary{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="log">The event log that records boundary lifecycle events.</param>
        public OpenRecordingBoundary(BoundaryEventLog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Add("open:begin");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Add("open:complete");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Add($"open:fault:{exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Add("open:cancel");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Add("open:dispose");
            return ValueTask.CompletedTask;
        }
    }
}
