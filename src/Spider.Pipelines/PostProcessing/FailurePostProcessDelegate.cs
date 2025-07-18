namespace Spider.Pipelines.PostProcessing
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Represents a delegate for executing post-processing logic when a pipeline operation fails.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being processed.</typeparam>
    /// <param name="context">
    /// The read-only context containing request data, pipeline state, and execution details.
    /// </param>
    /// <param name="arguments">
    /// The arguments for the post-processing step.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous post-processing operation.
    /// </returns>
    public delegate Task FailurePostProcessDelegate<TRequest>(
        IReadOnlyContext<TRequest> context,
        PostProcessArguments arguments);
}
