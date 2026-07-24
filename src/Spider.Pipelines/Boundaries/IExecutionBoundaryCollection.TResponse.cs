namespace Spider.Pipelines.Boundaries
{
    /// <summary>
    /// Configures execution boundaries that apply only to the current request/response invocation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IExecutionBoundaryCollection<TRequest, TResponse>
    {
        /// <summary>
        /// Adds a boundary instance to the current invocation.
        /// </summary>
        /// <param name="boundary">The boundary instance to apply.</param>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection<TRequest, TResponse> AddExecutionBoundary(IBoundary<TRequest, TResponse> boundary);

        /// <summary>
        /// Adds a delegate boundary to the current invocation.
        /// </summary>
        /// <param name="configure">The delegate boundary configuration.</param>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection<TRequest, TResponse> AddExecutionBoundary(Action<IExecutionBoundaryConfiguration<TRequest, TResponse>> configure);

        /// <summary>
        /// Adds a DI-resolved boundary type to the current invocation.
        /// </summary>
        /// <typeparam name="TBoundary">The boundary implementation type to resolve from DI.</typeparam>
        /// <returns>The current invocation boundary collection.</returns>
        IExecutionBoundaryCollection<TRequest, TResponse> AddExecutionBoundary<TBoundary>()
            where TBoundary : class, IBoundary<TRequest, TResponse>;
    }
}
