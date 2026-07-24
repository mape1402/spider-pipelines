namespace Spider.Pipelines.Samples.Basic
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Provides a sample execution boundary passed directly to a single invocation.
    /// </summary>
    public sealed class InvocationConsoleBoundary : IPipelineExecutionBoundary
    {
        private readonly SampleEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationConsoleBoundary"/> class.
        /// </summary>
        /// <param name="log">The sample event log.</param>
        public InvocationConsoleBoundary(SampleEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: begin {context.RequestType.Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write("invocation boundary: complete");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
        {
            _log.Write($"invocation boundary: fault {exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            _log.Write("invocation boundary: cancel");
            return ValueTask.CompletedTask;
        }
    }
}
