namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Configures execution boundaries that apply only to the current request-only invocation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IExecutionBoundaryCollection<TRequest>
    {
        /// <summary>
        /// Adds a boundary instance to the current invocation.
        /// </summary>
        /// <param name="boundary">The boundary instance to apply.</param>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection<TRequest> AddExecutionBoundary(IBoundary<TRequest> boundary);

        /// <summary>
        /// Adds a delegate boundary to the current invocation.
        /// </summary>
        /// <param name="configure">The delegate boundary configuration.</param>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection<TRequest> AddExecutionBoundary(Action<IExecutionBoundaryConfiguration<TRequest>> configure);

        /// <summary>
        /// Adds a DI-resolved boundary type to the current invocation.
        /// </summary>
        /// <typeparam name="TBoundary">The boundary implementation type to resolve from DI.</typeparam>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection<TRequest> AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IBoundary<TRequest>;
    }
}
