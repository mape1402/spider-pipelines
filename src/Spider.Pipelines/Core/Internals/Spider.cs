namespace Spider.Pipelines.Core.Internals
{
    using Microsoft.Extensions.DependencyInjection;
    using Spider.Pipelines.Architecture.Internals;
    using Spider.Pipelines.Flows;
    using Spider.Pipelines.Flows.Internals;

    /// <summary>
    /// Provides functionality to initialize service bridges and compose business flows.
    /// </summary>
    internal class InternalSpider : ISpider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SpiderArchitectureRegistry _architectureRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalSpider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public InternalSpider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _architectureRegistry = _serviceProvider.GetRequiredService<SpiderArchitectureRegistry>();
        }

        /// <inheritdoc/>
        public IServiceBridge<TService> InitBridge<TService>()
            => _serviceProvider.GetRequiredService<IServiceBridge<TService>>();

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TRequest> ComposeFlow<TRequest>(string name)
            => new SpiderFlowBuilder<TRequest, TRequest>(FlowBuilderState.Create<TRequest>(name, _architectureRegistry));

        /// <inheritdoc/>
        public ISpiderFlowBuilder<TRequest, TRequest, TResponse> ComposeFlow<TRequest, TResponse>(string name)
            => new SpiderFlowBuilder<TRequest, TRequest, TResponse>(FlowBuilderState.Create<TRequest, TResponse>(name, _architectureRegistry));
    }
}
