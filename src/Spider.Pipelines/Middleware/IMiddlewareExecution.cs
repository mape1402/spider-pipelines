namespace Spider.Pipelines.Middleware
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Defines execution for middleware steps in a request pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IMiddlewareExecution<TRequest>
    {
        /// <summary>
        /// Executes middleware around the supplied target operation.
        /// </summary>
        /// <param name="context">The current pipeline context.</param>
        /// <param name="target">The target operation to wrap.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnMiddlewareAsync(IReadOnlyContext<TRequest> context, Func<Task> target);
    }

    /// <summary>
    /// Defines execution for middleware steps in a request/response pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IMiddlewareExecution<TRequest, TResponse>
    {
        /// <summary>
        /// Executes middleware around the supplied target operation.
        /// </summary>
        /// <param name="context">The current pipeline context.</param>
        /// <param name="target">The target operation to wrap.</param>
        /// <returns>A task containing the response from the middleware chain.</returns>
        Task<TResponse> OnMiddlewareAsync(IReadOnlyContext<TRequest, TResponse> context, Func<Task<TResponse>> target);
    }
}
