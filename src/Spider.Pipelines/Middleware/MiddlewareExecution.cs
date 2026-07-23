namespace Spider.Pipelines.Middleware
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Executes configured middleware steps around a request pipeline target.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class MiddlewareExecution<TRequest> : IMiddlewareExecution<TRequest>
    {
        private readonly IReadOnlyCollection<PipelineMiddlewareDelegate<TRequest>> _middlewares;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareExecution{TRequest}"/> class.
        /// </summary>
        /// <param name="middlewares">The middleware delegates to execute.</param>
        public MiddlewareExecution(IReadOnlyCollection<PipelineMiddlewareDelegate<TRequest>> middlewares)
        {
            _middlewares = middlewares ?? Array.Empty<PipelineMiddlewareDelegate<TRequest>>();
        }

        /// <inheritdoc/>
        public Task OnMiddlewareAsync(IReadOnlyContext<TRequest> context, Func<Task> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Func<Task> next = target;

            foreach (var middleware in _middlewares.Reverse())
            {
                var current = next;
                next = () => middleware(context, current);
            }

            return next();
        }
    }

    /// <summary>
    /// Executes configured middleware steps around a request/response pipeline target.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class MiddlewareExecution<TRequest, TResponse> : IMiddlewareExecution<TRequest, TResponse>
    {
        private readonly IReadOnlyCollection<PipelineMiddlewareDelegate<TRequest, TResponse>> _middlewares;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareExecution{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="middlewares">The middleware delegates to execute.</param>
        public MiddlewareExecution(IReadOnlyCollection<PipelineMiddlewareDelegate<TRequest, TResponse>> middlewares)
        {
            _middlewares = middlewares ?? Array.Empty<PipelineMiddlewareDelegate<TRequest, TResponse>>();
        }

        /// <inheritdoc/>
        public Task<TResponse> OnMiddlewareAsync(IReadOnlyContext<TRequest, TResponse> context, Func<Task<TResponse>> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Func<Task<TResponse>> next = target;

            foreach (var middleware in _middlewares.Reverse())
            {
                var current = next;
                next = () => middleware(context, current);
            }

            return next();
        }
    }
}
