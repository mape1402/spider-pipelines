namespace Spider.Pipelines.RuntimeTracing
{
    using Spider.Pipelines.RuntimeTracing.Internals;

    /// <summary>
    /// Represents a started runtime trace span scope.
    /// </summary>
    public sealed class SpiderTraceScope : IAsyncDisposable
    {
        private readonly SpiderRuntimeTracer _tracer;
        private readonly SpiderTraceContext _context;
        private bool _completed;

        internal SpiderTraceScope(
            SpiderRuntimeTracer tracer,
            SpiderTraceContext context)
        {
            _tracer = tracer;
            _context = context;
        }

        /// <summary>
        /// Gets the trace identifier.
        /// </summary>
        public string TraceId => _context.TraceId;

        /// <summary>
        /// Gets the span identifier.
        /// </summary>
        public string SpanId => _context.SpanId;

        /// <summary>
        /// Completes the span successfully.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public ValueTask CompleteAsync(CancellationToken cancellationToken = default)
        {
            if (_completed)
                return ValueTask.CompletedTask;

            _completed = true;
            return _tracer.CompleteSpanAsync(_context, SpiderTraceStatus.Completed, null, cancellationToken);
        }

        /// <summary>
        /// Faults the span.
        /// </summary>
        /// <param name="exception">The exception that faulted the span.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public ValueTask FaultAsync(Exception exception, CancellationToken cancellationToken = default)
        {
            if (_completed)
                return ValueTask.CompletedTask;

            _completed = true;
            return _tracer.CompleteSpanAsync(_context, SpiderTraceStatus.Faulted, exception, cancellationToken);
        }

        /// <summary>
        /// Cancels the span.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public ValueTask CancelAsync(CancellationToken cancellationToken = default)
        {
            if (_completed)
                return ValueTask.CompletedTask;

            _completed = true;
            return _tracer.CompleteSpanAsync(_context, SpiderTraceStatus.Cancelled, null, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
            => _completed ? ValueTask.CompletedTask : CompleteAsync();
    }
}
