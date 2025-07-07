namespace Spider.Pipelines.PostProcessing
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Defines a contract for executing post-processing logic after the main pipeline operation for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IPostProcessExecution<TRequest>
    {
        /// <summary>
        /// Executes custom logic after a successful pipeline operation.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnSuccessAsync(IReadOnlyContext<TRequest> context);

        /// <summary>
        /// Executes custom logic after a failed pipeline operation.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnFailureAsync(IReadOnlyContext<TRequest> context);
    }

    /// <summary>
    /// Defines a contract for executing post-processing logic after the main pipeline operation for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IPostProcessExecution<TRequest, TResponse>
    {
        /// <summary>
        /// Executes custom logic after a successful pipeline operation.
        /// </summary>
        /// <param name="context">The read-only context containing the request, response, and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnSuccessAsync(IReadOnlyContext<TRequest, TResponse> context);

        /// <summary>
        /// Executes custom logic after a failed pipeline operation.
        /// </summary>
        /// <param name="context">The read-only context containing the request and pipeline state.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnFailureAsync(IReadOnlyContext<TRequest> context);
    }
}
