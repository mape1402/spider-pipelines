namespace Spider.Pipelines.Core
{
    using Spider.Pipelines.Flows;

    /// <summary>
    /// Defines a contract for initializing service bridges and composing business flows.
    /// </summary>
    public interface ISpider
    {
        /// <summary>
        /// Initializes a service bridge for the specified service type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to bridge.</typeparam>
        /// <returns>A service bridge for the specified service type.</returns>
        IServiceBridge<TService> InitBridge<TService>();

        /// <summary>
        /// Composes a business flow that executes a request without producing a response value.
        /// </summary>
        /// <typeparam name="TRequest">The request type that starts the flow.</typeparam>
        /// <param name="name">The logical flow name.</param>
        /// <returns>A flow builder initialized with the request as the active value.</returns>
        ISpiderFlowBuilder<TRequest, TRequest> ComposeFlow<TRequest>(string name);

        /// <summary>
        /// Composes a business flow that executes a request and produces a response value.
        /// </summary>
        /// <typeparam name="TRequest">The request type that starts the flow.</typeparam>
        /// <typeparam name="TResponse">The response type the flow must produce.</typeparam>
        /// <param name="name">The logical flow name.</param>
        /// <returns>A flow builder initialized with the request as the active value.</returns>
        ISpiderFlowBuilder<TRequest, TRequest, TResponse> ComposeFlow<TRequest, TResponse>(string name);
    }
}
