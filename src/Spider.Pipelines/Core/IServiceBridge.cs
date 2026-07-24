namespace Spider.Pipelines.Core
{
    using System.Linq.Expressions;
    using Spider.Pipelines.Boundaries;

    /// <summary>
    /// Defines a contract for bridging a service to pipeline execution and configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service being bridged.</typeparam>
    public interface IServiceBridge<TService>
    {
        /// <summary>
        /// Gets the service instance being bridged.
        /// </summary>
        TService Service { get; }

        /// <summary>
        /// Attaches a pipeline configuration for a specific request type.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="config">The configuration action for the pipeline builder.</param>
        /// <returns>A service bridge for the specified request type.</returns>
        IServiceBridge<TService, TRequest> Attach<TRequest>(Action<IPipelineBuilder<TRequest>> config);

        /// <summary>
        /// Attaches a pipeline configuration for a specific request and response type.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="config">The configuration action for the pipeline builder.</param>
        /// <returns>A service bridge for the specified request and response types.</returns>
        IServiceBridge<TService, TRequest, TResponse> Attach<TRequest, TResponse>(Action<IPipelineBuilder<TRequest, TResponse>> config);

        /// <summary>
        /// Executes the specified service delegate asynchronously with the given request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync<TRequest>(Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler, TRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified service delegate asynchronously with invocation-specific typed execution boundaries.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="executionBoundaries">The typed execution boundaries to apply only to this invocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync<TRequest>(
            Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest>> executionBoundaries,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified service delegate asynchronously with the given request and returns a response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the response as its result.</returns>
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler, TRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified service delegate asynchronously with invocation-specific typed execution boundaries.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="executionBoundaries">The typed execution boundaries to apply only to this invocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the response as its result.</returns>
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(
            Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest, TResponse>> executionBoundaries,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a contract for bridging a service to pipeline execution for a specific request type.
    /// </summary>
    /// <typeparam name="TService">The type of the service being bridged.</typeparam>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface IServiceBridge<TService, TRequest>
    {
        /// <summary>
        /// Executes the specified service delegate asynchronously with the given request.
        /// </summary>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler, TRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified service delegate asynchronously with invocation-specific typed execution boundaries.
        /// </summary>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="executionBoundaries">The typed execution boundaries to apply only to this invocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(
            Expression<ServiceInvokeDelegate<TService, TRequest>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest>> executionBoundaries,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a contract for bridging a service to pipeline execution for a specific request and response type.
    /// </summary>
    /// <typeparam name="TService">The type of the service being bridged.</typeparam>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface IServiceBridge<TService, TRequest, TResponse>
    {
        /// <summary>
        /// Executes the specified service delegate asynchronously with the given request and returns a response.
        /// </summary>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the response as its result.</returns>
        Task<TResponse> ExecuteAsync(Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler, TRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified service delegate asynchronously with invocation-specific typed execution boundaries.
        /// </summary>
        /// <param name="targetHandler">The service delegate expression to execute.</param>
        /// <param name="request">The request object.</param>
        /// <param name="executionBoundaries">The typed execution boundaries to apply only to this invocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the response as its result.</returns>
        Task<TResponse> ExecuteAsync(
            Expression<ServiceInvokeDelegate<TService, TRequest, TResponse>> targetHandler,
            TRequest request,
            IEnumerable<IBoundary<TRequest, TResponse>> executionBoundaries,
            CancellationToken cancellationToken = default);
    }
}
