namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Provides a base implementation for pipeline execution context, supporting cancellation and state management.
    /// </summary>
    public abstract class Context : IReadOnlyContext, ICancellableContext
    {
        private readonly object _syncRoot = new object();
        private bool _cancelled;
        private PipelineState _pipelineState;
        private ResultState _resultState;
        private Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="services">The service provider for dependency resolution.</param>
        /// <param name="cancellationToken">The cancellation token for the pipeline execution.</param>
        protected Context(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            _pipelineState = PipelineState.OnPreProcess;
            Services = services;
            CancellationToken = cancellationToken;
        }

        /// <inheritdoc/>
        public bool Cancelled
        {
            get
            {
                lock (_syncRoot)
                    return _cancelled;
            }
        }

        /// <inheritdoc/>
        public PipelineState PipelineState
        {
            get
            {
                lock (_syncRoot)
                    return _pipelineState;
            }
        }

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; }

        /// <inheritdoc/>
        public IServiceProvider Services { get; }

        /// <inheritdoc/>
        public ResultState ResultState
        {
            get
            {
                lock (_syncRoot)
                    return _resultState;
            }
        }

        /// <inheritdoc/>
        public Exception Exception
        {
            get
            {
                lock (_syncRoot)
                    return _exception;
            }
        }

        /// <summary>
        /// Synchronizes mutable context state shared between target and parallel steps.
        /// </summary>
        protected object SyncRoot => _syncRoot;

        /// <inheritdoc/>
        public void CancelOperation()
        {
            lock (_syncRoot)
                _cancelled = true;
        }

        /// <summary>
        /// Sets the pipeline state.
        /// </summary>
        /// <param name="state">The pipeline state to set.</param>
        protected void SetPipelineStateCore(PipelineState state)
        {
            lock (_syncRoot)
                _pipelineState = state;
        }

        /// <summary>
        /// Sets the result state.
        /// </summary>
        /// <param name="state">The result state to set.</param>
        protected void SetResultStateCore(ResultState state)
        {
            lock (_syncRoot)
                _resultState = state;
        }

        /// <summary>
        /// Sets the captured exception.
        /// </summary>
        /// <param name="exception">The exception to set.</param>
        protected void SetExceptionCore(Exception exception)
        {
            lock (_syncRoot)
                _exception = exception;
        }
    }

    /// <summary>
    /// Represents a pipeline execution context that includes a request object and supports state mutation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public class Context<TRequest> : Context, IReadOnlyContext<TRequest>, ISettableContext<TRequest>
    {
        private TRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context{TRequest}"/> class.
        /// </summary>
        /// <param name="request">The request object for the pipeline execution.</param>
        /// <param name="services">The service provider for dependency resolution.</param>
        /// <param name="cancellationToken">The cancellation token for the pipeline execution.</param>
        public Context(TRequest request, IServiceProvider services, CancellationToken cancellationToken = default) : base(services, cancellationToken)
        {
            _request = request;
        }

        /// <inheritdoc/>
        public TRequest Request
        {
            get
            {
                lock (SyncRoot)
                    return _request;
            }
        }

        /// <inheritdoc/>
        public void SetException(Exception exception)
        {
            SetExceptionCore(exception);
        }

        /// <inheritdoc/>
        public void SetPipelineState(PipelineState state)
        {
            SetPipelineStateCore(state);
        }

        /// <inheritdoc/>
        public void SetRequest(TRequest request)
        {
            lock (SyncRoot)
                _request = request;
        }

        /// <inheritdoc/>
        public void SetResultState(ResultState state)
        {
            SetResultStateCore(state);
        }
    }

    /// <summary>
    /// Represents a pipeline execution context that includes both request and response objects and supports state mutation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public sealed class Context<TRequest, TResponse> : Context<TRequest>, IReadOnlyContext<TRequest, TResponse>, ISettableContext<TRequest, TResponse>
    {
        private TResponse _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="request">The request object for the pipeline execution.</param>
        /// <param name="services">The service provider for dependency resolution.</param>
        /// <param name="cancellationToken">The cancellation token for the pipeline execution.</param>
        public Context(TRequest request, IServiceProvider services, CancellationToken cancellationToken = default) : base(request, services, cancellationToken) { }

        /// <inheritdoc/>
        public TResponse Response
        {
            get
            {
                lock (SyncRoot)
                    return _response;
            }
        }

        /// <inheritdoc/>
        public void SetResponse(TResponse response)
        {
            lock (SyncRoot)
                _response = response;
        }
    }
}
