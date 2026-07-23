namespace Spider.Pipelines.Middleware
{
    /// <summary>
    /// Defines configuration for middleware steps in a request pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IMiddlewareConfiguration<TRequest>
    {
        /// <summary>
        /// Adds middleware to the configuration.
        /// </summary>
        /// <param name="middleware">The middleware delegate to add.</param>
        /// <returns>The current configuration instance.</returns>
        IMiddlewareConfiguration<TRequest> Use(PipelineMiddlewareDelegate<TRequest> middleware);

        /// <summary>
        /// Builds execution logic for the configured middleware.
        /// </summary>
        /// <returns>A middleware execution instance.</returns>
        IMiddlewareExecution<TRequest> BuildExecution();
    }

    /// <summary>
    /// Defines configuration for middleware steps in a request/response pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IMiddlewareConfiguration<TRequest, TResponse>
    {
        /// <summary>
        /// Adds middleware to the configuration.
        /// </summary>
        /// <param name="middleware">The middleware delegate to add.</param>
        /// <returns>The current configuration instance.</returns>
        IMiddlewareConfiguration<TRequest, TResponse> Use(PipelineMiddlewareDelegate<TRequest, TResponse> middleware);

        /// <summary>
        /// Builds execution logic for the configured middleware.
        /// </summary>
        /// <returns>A middleware execution instance.</returns>
        IMiddlewareExecution<TRequest, TResponse> BuildExecution();
    }
}
