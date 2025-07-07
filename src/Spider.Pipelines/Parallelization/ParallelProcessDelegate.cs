namespace Spider.Pipelines.Parallelization
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Represents a delegate for executing a parallel processing step within the pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being processed.</typeparam>
    /// <param name="context">
    /// The read-only context containing the request, pipeline state, and shared services.
    /// </param>
    /// <param name="arguments">
    /// The arguments controlling the parallel execution behavior, such as degree of parallelism and cancellation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous parallel processing operation.
    /// </returns>
    public delegate Task ParallelProcessDelegate<TRequest>(
        IReadOnlyContext<TRequest> context,
        ParallelProcessArguments arguments);
}
