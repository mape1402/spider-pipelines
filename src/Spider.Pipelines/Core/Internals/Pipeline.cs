namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.Targeting;

    /// <summary>
    /// Represents a pipeline for a single request type, coordinating execution plan and context management.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class Pipeline<TRequest> : IPipeline<TRequest>
    {
        private readonly TargetHandler<TRequest> _targetHandler;
        private readonly IExecutionPlan<TRequest> _executionPlan;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline{TRequest}"/> class.
        /// </summary>
        /// <param name="targetHandler">The target handler delegate.</param>
        /// <param name="executionPlan">The execution plan for the pipeline.</param>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public Pipeline(TargetHandler<TRequest> targetHandler, IExecutionPlan<TRequest> executionPlan, IServiceProvider serviceProvider)
        {
            _targetHandler = targetHandler ?? throw new ArgumentNullException(nameof(targetHandler));
            _executionPlan = executionPlan ?? throw new ArgumentNullException(nameof(executionPlan));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public async Task RunAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var context = new Context<TRequest>(request, _serviceProvider, cancellationToken);
            context.OnPreProcess();

            await _executionPlan
                        .OnPreProcessAsync(context)
                        .ContinueWith(t =>
                        {
                            context.OnTargeting();
                            return _executionPlan.OnTargetingAsync(context, _targetHandler);
                        })
                        .Unwrap()
                        .ContinueWith(t =>
                        {
                            context.OnPostProcess();
                            return _executionPlan.OnPostProcessAsync(context);
                        })
                        .Unwrap();

            if (context.IsFailure())
                throw context.Exception;
        }
    }

    /// <summary>
    /// Represents a pipeline for a request and response type, coordinating execution plan and context management.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    internal sealed class Pipeline<TRequest, TResponse> : IPipeline<TRequest, TResponse>
    {
        private readonly TargetHandler<TRequest, TResponse> _targetHandler;
        private readonly IExecutionPlan<TRequest, TResponse> _executionPlan;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="targetHandler">The target handler delegate.</param>
        /// <param name="executionPlan">The execution plan for the pipeline.</param>
        /// <param name="serviceProvider">The service provider for dependency resolution.</param>
        public Pipeline(TargetHandler<TRequest, TResponse> targetHandler, IExecutionPlan<TRequest, TResponse> executionPlan, IServiceProvider serviceProvider)
        {
            _targetHandler = targetHandler ?? throw new ArgumentNullException(nameof(targetHandler));
            _executionPlan = executionPlan ?? throw new ArgumentNullException(nameof(executionPlan));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public async Task<TResponse> RunAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var context = new Context<TRequest, TResponse>(request, _serviceProvider, cancellationToken);
            context.OnPreProcess();

            var response = await _executionPlan
                        .OnPreProcessAsync(context)
                        .ContinueWith(t =>
                        {
                            context.OnTargeting();
                            return _executionPlan.OnTargetingAsync(context, _targetHandler);
                        })
                        .Unwrap()
                        .ContinueWith(async t =>
                        {
                            context.OnPostProcess();
                            await _executionPlan.OnPostProcessAsync(context);
                            return t.Result;
                        })
                        .Unwrap();

            if (context.IsFailure())
                throw context.Exception;

            return response;
        }
    }
}
