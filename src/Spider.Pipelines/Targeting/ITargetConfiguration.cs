namespace Spider.Pipelines.Targeting
{
    /// <summary>
    /// Defines a contract for configuring target handlers and building their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface ITargetConfiguration<TRequest>
    {
        /// <summary>
        /// Adds an override target handler and condition to the configuration.
        /// </summary>
        /// <param name="handler">The override target handler to add.</param>
        /// <param name="condition">The condition delegate that determines when the override handler should be used. Optional.</param>
        /// <inheritdoc/>
        ITargetConfiguration<TRequest> Overrides(TargetHandler<TRequest> handler, OverridesConditionDelegate<TRequest> condition = default);

        /// <summary>
        /// Builds the execution logic for the configured target handlers.
        /// </summary>
        /// <returns>An <see cref="ITargetExecution{TRequest}"/> instance that executes the configured target handlers.</returns>
        /// <inheritdoc/>
        ITargetExecution<TRequest> BuildExecution();
    }

    /// <summary>
    /// Defines a contract for configuring target handlers and building their execution logic for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface ITargetConfiguration<TRequest, TResponse>
    {
        /// <summary>
        /// Adds an override target handler and condition to the configuration.
        /// </summary>
        /// <param name="handler">The override target handler to add.</param>
        /// <param name="condition">The condition delegate that determines when the override handler should be used. Optional.</param>
        /// <inheritdoc/>
        ITargetConfiguration<TRequest, TResponse> Overrides(TargetHandler<TRequest, TResponse> handler, OverridesConditionDelegate<TRequest> condition = default);

        /// <summary>
        /// Builds the execution logic for the configured target handlers.
        /// </summary>
        /// <returns>An <see cref="ITargetExecution{TRequest, TResponse}"/> instance that executes the configured target handlers.</returns>
        /// <inheritdoc/>
        ITargetExecution<TRequest, TResponse> BuildExecution();
    }
}
