namespace Spider.Pipelines.Boundaries.Internals
{
    /// <summary>
    /// Provides a request-only execution boundary backed by configured delegates.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class DelegateExecutionBoundary<TRequest> : IBoundary<TRequest>, IExecutionBoundaryConfiguration<TRequest>
    {
        private Func<PipelineExecutionContext<TRequest>, CancellationToken, ValueTask> _onBegin;
        private Func<PipelineExecutionContext<TRequest>, CancellationToken, ValueTask> _onComplete;
        private Func<PipelineExecutionContext<TRequest>, Exception, CancellationToken, ValueTask> _onFault;
        private Func<PipelineExecutionContext<TRequest>, CancellationToken, ValueTask> _onCancel;
        private Func<PipelineExecutionContext<TRequest>, ValueTask> _onDispose;

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest> OnBegin(Func<PipelineExecutionContext<TRequest>, CancellationToken, ValueTask> handler)
        {
            _onBegin = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest> OnComplete(Func<PipelineExecutionContext<TRequest>, CancellationToken, ValueTask> handler)
        {
            _onComplete = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest> OnFault(Func<PipelineExecutionContext<TRequest>, Exception, CancellationToken, ValueTask> handler)
        {
            _onFault = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest> OnCancel(Func<PipelineExecutionContext<TRequest>, CancellationToken, ValueTask> handler)
        {
            _onCancel = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest> OnDispose(Func<PipelineExecutionContext<TRequest>, ValueTask> handler)
        {
            _onDispose = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
            => _onBegin == null ? ValueTask.CompletedTask : _onBegin(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
            => _onComplete == null ? ValueTask.CompletedTask : _onComplete(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext<TRequest> context, Exception exception, CancellationToken cancellationToken)
            => _onFault == null ? ValueTask.CompletedTask : _onFault(context, exception, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
            => _onCancel == null ? ValueTask.CompletedTask : _onCancel(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
            => _onDispose == null ? ValueTask.CompletedTask : _onDispose(context);
    }
}
