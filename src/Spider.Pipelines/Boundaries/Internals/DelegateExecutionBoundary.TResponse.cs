namespace Spider.Pipelines.Boundaries.Internals
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Provides a request/response execution boundary backed by configured delegates.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class DelegateExecutionBoundary<TRequest, TResponse> : IPipelineExecutionBoundary<TRequest, TResponse>, IExecutionBoundaryConfiguration<TRequest, TResponse>
    {
        private Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> _onBegin;
        private Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> _onComplete;
        private Func<IReadOnlyContext<TRequest, TResponse>, Exception, CancellationToken, ValueTask> _onFault;
        private Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> _onCancel;
        private Func<IReadOnlyContext<TRequest, TResponse>, ValueTask> _onDispose;

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnBegin(Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> handler)
        {
            _onBegin = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnComplete(Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> handler)
        {
            _onComplete = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnFault(Func<IReadOnlyContext<TRequest, TResponse>, Exception, CancellationToken, ValueTask> handler)
        {
            _onFault = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnCancel(Func<IReadOnlyContext<TRequest, TResponse>, CancellationToken, ValueTask> handler)
        {
            _onCancel = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public IExecutionBoundaryConfiguration<TRequest, TResponse> OnDispose(Func<IReadOnlyContext<TRequest, TResponse>, ValueTask> handler)
        {
            _onDispose = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <inheritdoc/>
        public ValueTask BeginAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onBegin == null ? ValueTask.CompletedTask : _onBegin(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CompleteAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onComplete == null ? ValueTask.CompletedTask : _onComplete(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask FaultAsync(IReadOnlyContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken)
            => _onFault == null ? ValueTask.CompletedTask : _onFault(context, exception, cancellationToken);

        /// <inheritdoc/>
        public ValueTask CancelAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onCancel == null ? ValueTask.CompletedTask : _onCancel(context, cancellationToken);

        /// <inheritdoc/>
        public ValueTask DisposeAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken)
            => _onDispose == null ? ValueTask.CompletedTask : _onDispose(context);
    }
}
