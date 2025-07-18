namespace Spider.Pipelines.Core
{
    using Spider.Pipelines.Targeting;

    /// <summary>
    /// Defines a contract for the execution plan of a pipeline with a request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IExecutionPlan<TRequest>
    {
        /// <summary>
        /// Executes preprocessing logic asynchronously.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnPreProcessAsync(IReadOnlyContext<TRequest> context);

        /// <summary>
        /// Executes targeting logic asynchronously using the provided target handler.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <param name="targetHandler">The target handler delegate to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnTargetingAsync(IReadOnlyContext<TRequest> context, TargetHandler<TRequest> targetHandler);

        /// <summary>
        /// Executes postprocessing logic asynchronously.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnPostProcessAsync(IReadOnlyContext<TRequest> context);
    }

    /// <summary>
    /// Defines a contract for the execution plan of a pipeline with a request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IExecutionPlan<TRequest, TResponse>
    {
        /// <summary>
        /// Executes preprocessing logic asynchronously.
        /// </summary>
        /// <param name="context">The read-only context containing the request, response, and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnPreProcessAsync(IReadOnlyContext<TRequest, TResponse> context);

        /// <summary>
        /// Executes targeting logic asynchronously using the provided target handler and returns a response.
        /// </summary>
        /// <param name="context">The read-only context containing the request, response, and pipeline state.</param>
        /// <param name="targetHandler">The target handler delegate to execute.</param>
        /// <returns>A task representing the asynchronous operation, with the response as its result.</returns>
        Task<TResponse> OnTargetingAsync(IReadOnlyContext<TRequest, TResponse> context, TargetHandler<TRequest, TResponse> targetHandler);

        /// <summary>
        /// Executes postprocessing logic asynchronously.
        /// </summary>
        /// <param name="context">The read-only context containing the request, response, and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnPostProcessAsync(IReadOnlyContext<TRequest, TResponse> context);
    }
}
