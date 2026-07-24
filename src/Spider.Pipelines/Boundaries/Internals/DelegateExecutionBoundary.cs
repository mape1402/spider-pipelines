namespace Spider.Pipelines.Boundaries.Internals
{
    /// <summary>
    /// Provides a provider-agnostic execution boundary backed by configured delegates.
    /// </summary>
    internal sealed class DelegateExecutionBoundary : IPipelineExecutionBoundary, IExecutionBoundaryConfiguration
    {
        private Func<PipelineExecutionContext, CancellationToken, ValueTask> _onBegin;
        private Func<PipelineExecutionContext, CancellationToken, ValueTask> _onComplete;
        private Func<PipelineExecutionContext, Exception, CancellationToken, ValueTask> _onFault;
        private Func<PipelineExecutionContext, CancellationToken, ValueTask> _onCancel;
        private Func<PipelineExecutionContext, ValueTask> _onDispose;

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration OnBegin(Func<PipelineExecutionContext, CancellationToken, ValueTask> handler)
        {
            _onBegin = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration OnComplete(Func<PipelineExecutionContext, CancellationToken, ValueTask> handler)
        {
            _onComplete = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration OnFault(Func<PipelineExecutionContext, Exception, CancellationToken, ValueTask> handler)
        {
            _onFault = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration OnCancel(Func<PipelineExecutionContext, CancellationToken, ValueTask> handler)
        {
            _onCancel = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration OnDispose(Func<PipelineExecutionContext, ValueTask> handler)
        {
            _onDispose = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => _onBegin == null ? ValueTask.CompletedTask : _onBegin(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => _onComplete == null ? ValueTask.CompletedTask : _onComplete(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
            => _onFault == null ? ValueTask.CompletedTask : _onFault(context, exception, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
            => _onCancel == null ? ValueTask.CompletedTask : _onCancel(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask DisposeAsync(PipelineExecutionContext context)
            => _onDispose == null ? ValueTask.CompletedTask : _onDispose(context);
    }
}
