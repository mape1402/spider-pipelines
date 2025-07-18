namespace Spider.Pipelines.PostProcessing
{
    /// <summary>
    /// Defines a contract for configuring post-processing steps and building their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IPostProcessConfiguration<TRequest>
    {
        /// <summary>
        /// Adds a post-processing delegate to be executed after a successful operation.
        /// </summary>
        /// <param name="handler">The delegate to execute after a successful operation.</param>
        /// <inheritdoc/>
        IPostProcessConfiguration<TRequest> OnSuccess(SuccessPostProcessDelegate<TRequest> handler);

        /// <summary>
        /// Adds a post-processing delegate to be executed after a failed operation.
        /// </summary>
        /// <param name="handler">The delegate to execute after a failed operation.</param>
        /// <inheritdoc/>
        IPostProcessConfiguration<TRequest> OnFailure(FailurePostProcessDelegate<TRequest> handler);

        /// <summary>
        /// Builds the execution logic for the configured post-processing steps.
        /// </summary>
        /// <returns>An <see cref="IPostProcessExecution{TRequest}"/> instance that executes the configured post-processing steps.</returns>
        /// <inheritdoc/>
        IPostProcessExecution<TRequest> BuildExecution();
    }

    /// <summary>
    /// Defines a contract for configuring post-processing steps and building their execution logic for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IPostProcessConfiguration<TRequest, TResponse>
    {
        /// <summary>
        /// Adds a post-processing delegate to be executed after a successful operation.
        /// </summary>
        /// <param name="handler">The delegate to execute after a successful operation.</param>
        /// <inheritdoc/>
        IPostProcessConfiguration<TRequest, TResponse> OnSuccess(SuccessPostProcessDelegate<TRequest, TResponse> handler);

        /// <summary>
        /// Adds a post-processing delegate to be executed after a failed operation.
        /// </summary>
        /// <param name="handler">The delegate to execute after a failed operation.</param>
        /// <inheritdoc/>
        IPostProcessConfiguration<TRequest, TResponse> OnFailure(FailurePostProcessDelegate<TRequest> handler);

        /// <summary>
        /// Builds the execution logic for the configured post-processing steps.
        /// </summary>
        /// <returns>An <see cref="IPostProcessExecution{TRequest, TResponse}"/> instance that executes the configured post-processing steps.</returns>
        /// <inheritdoc/>
        IPostProcessExecution<TRequest, TResponse> BuildExecution();
    }
}
