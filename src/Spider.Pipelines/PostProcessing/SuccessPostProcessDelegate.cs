namespace Spider.Pipelines.PostProcessing
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Represents a delegate for executing post-processing logic after a successful pipeline operation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being processed.</typeparam>
    /// <param name="context">
    /// The read-only context containing request data and pipeline state.
    /// </param>
    /// <param name="arguments">
    /// The arguments for the post-processing step.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous post-processing operation.
    /// </returns>
    public delegate Task SuccessPostProcessDelegate<TRequest>(
        IReadOnlyContext<TRequest> context,
        PostProcessArguments arguments);

    /// <summary>
    /// Represents a delegate for executing post-processing logic after a successful pipeline operation,
    /// including both the request and response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being processed.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by the operation.</typeparam>
    /// <param name="context">
    /// The read-only context containing both the request and response, as well as pipeline state.
    /// </param>
    /// <param name="arguments">
    /// The arguments for the post-processing step.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous post-processing operation.
    /// </returns>
    public delegate Task SuccessPostProcessDelegate<TRequest, TResponse>(
        IReadOnlyContext<TRequest, TResponse> context,
        PostProcessArguments arguments);
}