namespace Spider.Pipelines.Targeting
{
    /// <summary>
    /// Represents a delegate that handles a target operation with a request but no response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <param name="request">The request object to process.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public delegate Task TargetHandler<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Represents a delegate that handles a target operation with a request and produces a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request object to process.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation and contains the response.</returns>
    public delegate Task<TResponse> TargetHandler<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default);
}