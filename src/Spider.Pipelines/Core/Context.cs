namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Provides a base implementation for pipeline execution context, supporting cancellation and state management.
    /// </summary>
    public abstract class Context : IReadOnlyContext, ICancellableContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="services">The service provider for dependency resolution.</param>
        /// <param name="cancellationToken">The cancellation token for the pipeline execution.</param>
        protected Context(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            Cancelled = false;
            PipelineState = PipelineState.OnPreProcess;
            Services = services;
            CancellationToken = cancellationToken;
        }

        /// <inheritdoc/>
        public bool Cancelled { get; private set; }

        /// <inheritdoc/>
        public PipelineState PipelineState { get; protected set; }

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; }

        /// <inheritdoc/>
        public IServiceProvider Services { get; }

        /// <inheritdoc/>
        public ResultState ResultState { get; protected set; }

        /// <inheritdoc/>
        public Exception Exception { get; protected set; }

        /// <inheritdoc/>
        public void CancelOperation()
        {
            Cancelled = true;
        }
    }

    /// <summary>
    /// Represents a pipeline execution context that includes a request object and supports state mutation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public class Context<TRequest> : Context, IReadOnlyContext<TRequest>, ISettableContext<TRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context{TRequest}"/> class.
        /// </summary>
        /// <param name="request">The request object for the pipeline execution.</param>
        /// <param name="services">The service provider for dependency resolution.</param>
        /// <param name="cancellationToken">The cancellation token for the pipeline execution.</param>
        public Context(TRequest request, IServiceProvider services, CancellationToken cancellationToken = default) : base(services, cancellationToken)
        {
            Request = request;
        }

        /// <inheritdoc/>
        public TRequest Request { get; private set; }

        /// <inheritdoc/>
        public void SetException(Exception exception)
        {
            Exception = exception;
        }

        /// <inheritdoc/>
        public void SetPipelineState(PipelineState state)
        {
            PipelineState = state;
        }

        /// <inheritdoc/>
        public void SetRequest(TRequest request)
        {
            Request = request;
        }

        /// <inheritdoc/>
        public void SetResultState(ResultState state)
        {
            ResultState = state;
        }
    }

    /// <summary>
    /// Represents a pipeline execution context that includes both request and response objects and supports state mutation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public sealed class Context<TRequest, TResponse> : Context<TRequest>, IReadOnlyContext<TRequest, TResponse>, ISettableContext<TRequest, TResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="request">The request object for the pipeline execution.</param>
        /// <param name="services">The service provider for dependency resolution.</param>
        /// <param name="cancellationToken">The cancellation token for the pipeline execution.</param>
        public Context(TRequest request, IServiceProvider services, CancellationToken cancellationToken = default) : base(request, services, cancellationToken) { }

        /// <inheritdoc/>
        public TResponse Response { get; private set; }

        /// <inheritdoc/>
        public void SetResponse(TResponse response)
        {
            Response = response;
        }
    }
}
