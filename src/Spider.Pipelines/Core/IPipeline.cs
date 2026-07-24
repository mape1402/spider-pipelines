namespace Spider.Pipelines.Core
{
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Defines a contract for running a pipeline with a request.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IPipeline<TRequest>
    {
        /// <summary>
        /// Runs the pipeline asynchronously with the specified request.
        /// </summary>
        /// <param name="request">The request object to process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunAsync(TRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs the pipeline asynchronously with typed execution boundaries provided for this invocation.
        /// </summary>
        /// <param name="request">The request object to process.</param>
        /// <param name="executionBoundaries">The typed execution boundaries to apply only to this invocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunAsync(TRequest request, IEnumerable<IPipelineExecutionBoundary<TRequest>> executionBoundaries, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a contract for running a pipeline with a request and returning a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IPipeline<TRequest, TResponse>
    {
        /// <summary>
        /// Runs the pipeline asynchronously with the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request object to process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the response as its result.</returns>
        Task<TResponse> RunAsync(TRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs the pipeline asynchronously with typed execution boundaries provided for this invocation.
        /// </summary>
        /// <param name="request">The request object to process.</param>
        /// <param name="executionBoundaries">The typed execution boundaries to apply only to this invocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the response as its result.</returns>
        Task<TResponse> RunAsync(TRequest request, IEnumerable<IPipelineExecutionBoundary<TRequest, TResponse>> executionBoundaries, CancellationToken cancellationToken = default);
    }
}
