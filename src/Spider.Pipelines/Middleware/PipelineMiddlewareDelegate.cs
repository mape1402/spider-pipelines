namespace Spider.Pipelines.Middleware
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Represents middleware that wraps execution of a request pipeline target.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <param name="context">The current pipeline context.</param>
    /// <param name="next">The next operation in the middleware chain.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public delegate Task PipelineMiddlewareDelegate<TRequest>(
        IReadOnlyContext<TRequest> context,
        Func<Task> next);

    /// <summary>
    /// Represents middleware that wraps execution of a request/response pipeline target.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    /// <param name="context">The current pipeline context.</param>
    /// <param name="next">The next operation in the middleware chain.</param>
    /// <returns>A task containing the response from the middleware chain.</returns>
    public delegate Task<TResponse> PipelineMiddlewareDelegate<TRequest, TResponse>(
        IReadOnlyContext<TRequest, TResponse> context,
        Func<Task<TResponse>> next);
}
