namespace Spider.Pipelines.PreProcessing
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Represents a delegate for a preprocessor step that runs before the main operation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being processed.</typeparam>
    /// <param name="request">
    /// The read-only context containing the request and pipeline state.
    /// </param>
    /// <param name="arguments">
    /// The arguments for the preprocessor, including the ability to cancel the pipeline.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous preprocessor execution.
    /// </returns>
    public delegate Task PreProcessDelegate<TRequest>(
        IReadOnlyContext<TRequest> request,
        PreProcessArguments arguments);
}
