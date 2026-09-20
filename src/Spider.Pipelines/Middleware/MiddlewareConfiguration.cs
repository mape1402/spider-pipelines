namespace Spider.Pipelines.Middleware
{
    using System.Collections.Immutable;

    /// <summary>
    /// Provides configuration for middleware steps in a request pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class MiddlewareConfiguration<TRequest> : IMiddlewareConfiguration<TRequest>
    {
        private readonly IList<PipelineMiddlewareDelegate<TRequest>> _middlewares;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareConfiguration{TRequest}"/> class.
        /// </summary>
        public MiddlewareConfiguration()
        {
            _middlewares = new List<PipelineMiddlewareDelegate<TRequest>>();
        }

        /// <inheritdoc/>
        public IMiddlewareConfiguration<TRequest> Use(PipelineMiddlewareDelegate<TRequest> middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException(nameof(middleware));

            _middlewares.Add(middleware);
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareExecution<TRequest> BuildExecution()
            => new MiddlewareExecution<TRequest>(_middlewares.ToImmutableArray());
    }

    /// <summary>
    /// Provides configuration for middleware steps in a request/response pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class MiddlewareConfiguration<TRequest, TResponse> : IMiddlewareConfiguration<TRequest, TResponse>
    {
        private readonly IList<PipelineMiddlewareDelegate<TRequest, TResponse>> _middlewares;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareConfiguration{TRequest, TResponse}"/> class.
        /// </summary>
        public MiddlewareConfiguration()
        {
            _middlewares = new List<PipelineMiddlewareDelegate<TRequest, TResponse>>();
        }

        /// <inheritdoc/>
        public IMiddlewareConfiguration<TRequest, TResponse> Use(PipelineMiddlewareDelegate<TRequest, TResponse> middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException(nameof(middleware));

            _middlewares.Add(middleware);
            return this;
        }

        /// <inheritdoc/>
        public IMiddlewareExecution<TRequest, TResponse> BuildExecution()
            => new MiddlewareExecution<TRequest, TResponse>(_middlewares.ToImmutableArray());
    }
}
