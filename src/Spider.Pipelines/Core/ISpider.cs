namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Defines a contract for initializing a service bridge to pipeline execution and configuration.
    /// </summary>
    public interface ISpider
    {
        /// <summary>
        /// Initializes a service bridge for the specified service type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to bridge.</typeparam>
        /// <returns>A service bridge for the specified service type.</returns>
        IServiceBridge<TService> InitBridge<TService>();
    }
}
