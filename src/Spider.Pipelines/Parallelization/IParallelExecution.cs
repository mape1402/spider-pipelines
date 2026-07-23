namespace Spider.Pipelines.Parallelization
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Defines a contract for executing parallel processing logic in the pipeline for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IParallelExecution<TRequest>
    {
        /// <summary>
        /// Executes parallel processing logic asynchronously for the specified request context.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnParallelAsync(IReadOnlyContext<TRequest> context);
    }

    /// <summary>
    /// Defines a contract for executing parallel processing logic in the pipeline for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IParallelExecution<TRequest, TResponse>
    {
        /// <summary>
        /// Executes parallel processing logic asynchronously for the specified request and response context.
        /// </summary>
        /// <param name="context">The read-only context containing the request, response, and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnParallelAsync(IReadOnlyContext<TRequest, TResponse> context);
    }
}
