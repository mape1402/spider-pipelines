namespace Spider.Pipelines.Boundaries.Internals
{
    /// <summary>
    /// Provides a request/response execution boundary backed by configured delegates.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class DelegateExecutionBoundary<TRequest, TResponse> : IBoundary<TRequest, TResponse>, IExecutionBoundaryConfiguration<TRequest, TResponse>
    {
        private Func<PipelineExecutionContext<TRequest, TResponse>, CancellationToken, ValueTask> _onBegin;
        private Func<PipelineExecutionContext<TRequest, TResponse>, CancellationToken, ValueTask> _onComplete;
        private Func<PipelineExecutionContext<TRequest, TResponse>, Exception, CancellationToken, ValueTask> _onFault;
        private Func<PipelineExecutionContext<TRequest, TResponse>, CancellationToken, ValueTask> _onCancel;
        private Func<PipelineExecutionContext<TRequest, TResponse>, ValueTask> _onDispose;

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnBegin(Func<PipelineExecutionContext<TRequest, TResponse>, CancellationToken, ValueTask> handler)
        {
            _onBegin = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnComplete(Func<PipelineExecutionContext<TRequest, TResponse>, CancellationToken, ValueTask> handler)
        {
            _onComplete = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnFault(Func<PipelineExecutionContext<TRequest, TResponse>, Exception, CancellationToken, ValueTask> handler)
        {
            _onFault = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnCancel(Func<PipelineExecutionContext<TRequest, TResponse>, CancellationToken, ValueTask> handler)
        {
            _onCancel = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnDispose(Func<PipelineExecutionContext<TRequest, TResponse>, ValueTask> handler)
        {
            _onDispose = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onBegin == null ? ValueTask.CompletedTask : _onBegin(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onComplete == null ? ValueTask.CompletedTask : _onComplete(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken)
            => _onFault == null ? ValueTask.CompletedTask : _onFault(context, exception, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onCancel == null ? ValueTask.CompletedTask : _onCancel(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onDispose == null ? ValueTask.CompletedTask : _onDispose(context);
    }
}
