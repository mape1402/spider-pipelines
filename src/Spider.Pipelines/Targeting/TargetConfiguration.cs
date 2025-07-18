namespace Spider.Pipelines.Targeting
{
    /// <summary>
    /// Provides configuration for target handlers and builds their execution logic for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class TargetConfiguration<TRequest> : ITargetConfiguration<TRequest>
    {
        /// <summary>
        /// The service provider for dependency resolution.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// The override target handler to execute if the override condition is met.
        /// </summary>
        private TargetHandler<TRequest> _targetHandler;
        /// <summary>
        /// The condition delegate that determines when the override handler should be used.
        /// </summary>
        private OverridesConditionDelegate<TRequest> _overridesCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetConfiguration{TRequest}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public TargetConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public ITargetConfiguration<TRequest> Overrides(TargetHandler<TRequest> handler, OverridesConditionDelegate<TRequest> condition = null)
        {
            _targetHandler = handler;
            _overridesCondition = condition;

            return this;
        }

        /// <inheritdoc/>
        public ITargetExecution<TRequest> BuildExecution()
            => new TargetExecution<TRequest>(_targetHandler, _overridesCondition);
    }

    /// <summary>
    /// Provides configuration for target handlers and builds their execution logic for a given request and response type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class TargetConfiguration<TRequest, TResponse> : ITargetConfiguration<TRequest, TResponse>
    {
        /// <summary>
        /// The service provider for dependency resolution.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// The override target handler to execute if the override condition is met.
        /// </summary>
        private TargetHandler<TRequest, TResponse> _targetHandler;
        /// <summary>
        /// The condition delegate that determines when the override handler should be used.
        /// </summary>
        private OverridesConditionDelegate<TRequest> _overridesCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetConfiguration{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public TargetConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public ITargetConfiguration<TRequest, TResponse> Overrides(TargetHandler<TRequest, TResponse> handler, OverridesConditionDelegate<TRequest> condition = null)
        {
            _targetHandler = handler;
            _overridesCondition = condition;

            return this;
        }

        /// <inheritdoc/>
        public ITargetExecution<TRequest, TResponse> BuildExecution()
            => new TargetExecution<TRequest, TResponse>(_targetHandler, _overridesCondition);
    }
}
