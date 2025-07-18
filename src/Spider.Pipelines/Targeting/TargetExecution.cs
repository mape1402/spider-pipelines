namespace Spider.Pipelines.Targeting
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;

    /// <summary>
    /// Executes a target handler or an override handler for a given request type in the pipeline, based on an override condition.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal class TargetExecution<TRequest> : ITargetExecution<TRequest>
    {
        private readonly TargetHandler<TRequest> _overridesHandler;
        private readonly OverridesConditionDelegate<TRequest> _overridesCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetExecution{TRequest}"/> class.
        /// </summary>
        /// <param name="overridesHandler">The override handler to execute if the override condition is met.</param>
        /// <param name="overridesCondition">The delegate that determines whether the override handler should be executed.</param>
        public TargetExecution(TargetHandler<TRequest> overridesHandler, OverridesConditionDelegate<TRequest> overridesCondition)
        {
            _overridesHandler = overridesHandler ?? throw new ArgumentNullException(nameof(overridesHandler));
            _overridesCondition = overridesCondition ?? throw new ArgumentNullException(nameof(overridesCondition)); 
        }

        /// <inheritdoc/>
        public Task OnTargetExecution(IReadOnlyContext<TRequest> context, TargetHandler<TRequest> targetHandler)
        {
            if (context.IsCancelled())
                return Task.CompletedTask;

            if (_overridesHandler == null || _overridesCondition == null)
                return targetHandler(context.Request, context.CancellationToken);

            //TODO: Get standard configuration
            var arguments = new OverridesConditionArguments();

            if (_overridesCondition(context, arguments))
                return _overridesHandler(context.Request, context.CancellationToken);
            else
                return targetHandler(context.Request, context.CancellationToken);
        }
    }

    /// <summary>
    /// Executes a target handler or an override handler for a given request and response type in the pipeline, based on an override condition.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal class TargetExecution<TRequest, TResponse> : ITargetExecution<TRequest, TResponse>
    {
        private readonly TargetHandler<TRequest, TResponse> _overridesHandler;
        private readonly OverridesConditionDelegate<TRequest> _overridesCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetExecution{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="overridesHandler">The override handler to execute if the override condition is met.</param>
        /// <param name="overridesCondition">The delegate that determines whether the override handler should be executed.</param>
        public TargetExecution(TargetHandler<TRequest, TResponse> overridesHandler, OverridesConditionDelegate<TRequest> overridesCondition)
        {
            _overridesHandler = overridesHandler ?? throw new ArgumentNullException(nameof(overridesHandler));
            _overridesCondition = overridesCondition ?? throw new ArgumentNullException(nameof(overridesCondition));
        }

        /// <inheritdoc/>
        public Task<TResponse> OnTargetExecution(IReadOnlyContext<TRequest, TResponse> context, TargetHandler<TRequest, TResponse> targetHandler)
        {
            if (context.IsCancelled())
                return Task.FromResult(default(TResponse));

            if (_overridesHandler == null || _overridesCondition == null)
                return targetHandler(context.Request, context.CancellationToken);

            //TODO: Get standard configuration
            var arguments = new OverridesConditionArguments();

            if (_overridesCondition(context, arguments))
                return _overridesHandler(context.Request, context.CancellationToken);
            else
                return targetHandler(context.Request, context.CancellationToken);
        }
    }
}
