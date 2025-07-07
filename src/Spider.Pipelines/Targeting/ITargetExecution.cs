namespace Spider.Pipelines.Targeting
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Defines a contract for executing a target handler in the pipeline for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface ITargetExecution<TRequest>
    {
        /// <summary>
        /// Executes the specified target handler asynchronously for the given request context.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <param name="targetHandler">The target handler delegate to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnTargetExecution(IReadOnlyContext<TRequest> context, TargetHandler<TRequest> targetHandler);
    }

    /// <summary>
    /// Defines a contract for executing a target handler in the pipeline for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface ITargetExecution<TRequest, TResponse>
    {
        /// <summary>
        /// Executes the specified target handler asynchronously for the given request and response context.
        /// </summary>
        /// <param name="context">The read-only context containing the request, response, and pipeline state.</param>
        /// <param name="targetHandler">The target handler delegate to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnTargetExecution(IReadOnlyContext<TRequest, TResponse> context, TargetHandler<TRequest, TResponse> targetHandler);
    }
}
