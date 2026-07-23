namespace Spider.Pipelines.Parallelization
{
    /// <summary>
    /// Defines a contract for configuring parallel processing steps and building their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IParallelConfiguration<TRequest>
    {
        /// <summary>
        /// Adds a parallel processing delegate to the configuration.
        /// </summary>
        /// <param name="handler">The parallel processing delegate to add.</param>
        /// <inheritdoc/>
        IParallelConfiguration<TRequest> OnParallel(ParallelProcessDelegate<TRequest> handler);

        /// <summary>
        /// Configures when parallel steps run relative to the target handler.
        /// </summary>
        /// <param name="mode">The execution mode to use.</param>
        /// <returns>The current configuration instance.</returns>
        IParallelConfiguration<TRequest> WithMode(ParallelExecutionMode mode);

        /// <summary>
        /// Builds the execution logic for the configured parallel processing steps.
        /// </summary>
        /// <returns>An <see cref="IParallelExecution{TRequest}"/> instance that executes the configured parallel steps.</returns>
        /// <inheritdoc/>
        IParallelExecution<TRequest> BuildExecution();
    }

    /// <summary>
    /// Defines a contract for configuring parallel processing steps and building their execution logic for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IParallelConfiguration<TRequest, TResponse>
    {
        /// <summary>
        /// Adds a parallel processing delegate to the configuration.
        /// </summary>
        /// <param name="handler">The parallel processing delegate to add.</param>
        /// <inheritdoc/>
        IParallelConfiguration<TRequest, TResponse> OnParallel(ParallelProcessDelegate<TRequest> handler);

        /// <summary>
        /// Configures when parallel steps run relative to the target handler.
        /// </summary>
        /// <param name="mode">The execution mode to use.</param>
        /// <returns>The current configuration instance.</returns>
        IParallelConfiguration<TRequest, TResponse> WithMode(ParallelExecutionMode mode);

        /// <summary>
        /// Builds the execution logic for the configured parallel processing steps.
        /// </summary>
        /// <returns>An <see cref="IParallelExecution{TRequest, TResponse}"/> instance that executes the configured parallel steps.</returns>
        /// <inheritdoc/>
        IParallelExecution<TRequest, TResponse> BuildExecution();
    }
}
