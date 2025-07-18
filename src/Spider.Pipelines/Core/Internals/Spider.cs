namespace Spider.Pipelines.Core.Internals
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides functionality to initialize service bridges for pipeline execution and configuration.
    /// </summary>
    internal class InternalSpider : ISpider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public InternalSpider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public IServiceBridge<TService> InitBridge<TService>()
            => _serviceProvider.GetRequiredService<IServiceBridge<TService>>();
    }
}
