using Spider.Pipelines.Targeting;

namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Represents a delegate that creates a target handler for a service operation 
    /// that processes a request without returning a response.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <param name="service">The service instance to invoke.</param>
    /// <returns>A <see cref="TargetHandler{TRequest}"/> that handles the operation.</returns>
    public delegate TargetHandler<TRequest> ServiceInvokeDelegate<TService, TRequest>(TService service);

    /// <summary>
    /// Represents a delegate that creates a target handler for a service operation 
    /// that processes a request and produces a response.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="service">The service instance to invoke.</param>
    /// <returns>
    /// A <see cref="TargetHandler{TRequest, TResponse}"/> that handles the operation.
    /// </returns>
    public delegate TargetHandler<TRequest, TResponse> ServiceInvokeDelegate<TService, TRequest, TResponse>(TService service);
}
