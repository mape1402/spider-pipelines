namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Defines a read-only execution context for a pipeline step.
    /// </summary>
    public interface IReadOnlyContext
    {
        /// <summary>
        /// Provides access to the current <see cref="IServiceProvider"/>.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Indicates whether the pipeline has been cancelled.
        /// </summary>
        bool Cancelled { get; }

        /// <summary>
        /// Gets the current state of the pipeline step.
        /// </summary>
        PipelineState PipelineState { get; }

        /// <summary>
        /// The <see cref="CancellationToken"/> associated with the pipeline execution.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Represents the result state of the operation.
        /// </summary>
        ResultState ResultState { get; }

        /// <summary>
        /// Contains any exception thrown during pipeline execution, if applicable.
        /// </summary>
        Exception Exception { get; }
    }

    /// <summary>
    /// Defines a read-only execution context that includes the request.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IReadOnlyContext<TRequest> : IReadOnlyContext
    {
        /// <summary>
        /// Gets the request associated with the pipeline execution.
        /// </summary>
        TRequest Request { get; }
    }

    /// <summary>
    /// Defines a read-only execution context that includes both the request and response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IReadOnlyContext<TRequest, TResponse> : IReadOnlyContext<TRequest>
    {
        /// <summary>
        /// Gets the response associated with the pipeline execution.
        /// </summary>
        TResponse Response { get; }
    }
}