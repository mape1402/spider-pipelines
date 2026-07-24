namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a globally registered sample execution boundary for any request/response pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public sealed class GlobalConsoleBoundary<TRequest, TResponse> : IBoundary<TRequest, TResponse>
    {
        private readonly SampleEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalConsoleBoundary{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="log">The sample event log.</param>
        public GlobalConsoleBoundary(SampleEventLog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: begin {typeof(TRequest).Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: complete {typeof(TResponse).Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Write($"global boundary: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Write("global boundary: cancel");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            _log.Write("global boundary: dispose");
            return ValueTask.CompletedTask;
        }
    }
}
