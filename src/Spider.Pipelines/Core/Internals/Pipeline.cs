namespace Spider.Pipelines.Core.Internals
{
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Boundaries.Internals;
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
            => await RunAsync(request, Array.Empty<IPipelineExecutionBoundary<TRequest>>(), cancellationToken);

        /// <inheritdoc/>
        public async Task RunAsync(TRequest request, IEnumerable<IPipelineExecutionBoundary<TRequest>> executionBoundaries, CancellationToken cancellationToken = default)
        {
            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var context = new Context<TRequest>(request, _serviceProvider, cancellationToken);
            var boundaryRunner = new PipelineExecutionBoundaryRunner(_serviceProvider);
            var pipelineBoundaries = CreatePipelineBoundaries();
            var invocationBoundaries = executionBoundaries.ToArray();

            await boundaryRunner.RunAsync(
                context,
                () => RunCoreAsync(context),
                pipelineBoundaries.Concat(invocationBoundaries),
                cancellationToken);
        }

        /// <summary>
        /// Runs the request-only pipeline core inside any registered execution boundaries.
        /// </summary>
        /// <param name="context">The current pipeline context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RunCoreAsync(Context<TRequest> context)
        {
            context.OnPreProcess();

            await _executionPlan.OnPreProcessAsync(context);

            context.OnTargeting();
            await _executionPlan.OnTargetingAsync(context, _targetHandler);

            context.OnPostProcess();
            await _executionPlan.OnPostProcessAsync(context);

            if (context.IsFailure())
                throw context.Exception;
        }

        /// <summary>
        /// Resolves boundaries configured through the fluent pipeline builder.
        /// </summary>
        /// <returns>The execution boundaries configured for this pipeline.</returns>
        private IReadOnlyCollection<IPipelineExecutionBoundary<TRequest>> CreatePipelineBoundaries()
            => _executionPlan is IExecutionBoundaryPlan<TRequest> boundaryPlan
                ? boundaryPlan.CreateExecutionBoundaries(_serviceProvider)
                : Array.Empty<IPipelineExecutionBoundary<TRequest>>();
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
            => await RunAsync(request, Array.Empty<IPipelineExecutionBoundary<TRequest, TResponse>>(), cancellationToken);

        /// <inheritdoc/>
        public async Task<TResponse> RunAsync(TRequest request, IEnumerable<IPipelineExecutionBoundary<TRequest, TResponse>> executionBoundaries, CancellationToken cancellationToken = default)
        {
            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var context = new Context<TRequest, TResponse>(request, _serviceProvider, cancellationToken);
            var boundaryRunner = new PipelineExecutionBoundaryRunner(_serviceProvider);
            var pipelineBoundaries = CreatePipelineBoundaries();
            var invocationBoundaries = executionBoundaries.ToArray();

            return await boundaryRunner.RunAsync(
                context,
                () => RunCoreAsync(context),
                pipelineBoundaries.Concat(invocationBoundaries),
                cancellationToken);
        }

        /// <summary>
        /// Runs the request/response pipeline core inside any registered execution boundaries.
        /// </summary>
        /// <param name="context">The current pipeline context.</param>
        /// <returns>A task containing the pipeline response.</returns>
        private async Task<TResponse> RunCoreAsync(Context<TRequest, TResponse> context)
        {
            context.OnPreProcess();

            await _executionPlan.OnPreProcessAsync(context);

            context.OnTargeting();
            var response = await _executionPlan.OnTargetingAsync(context, _targetHandler);

            context.OnPostProcess();
            await _executionPlan.OnPostProcessAsync(context);

            if (context.IsFailure())
                throw context.Exception;

            return response;
        }

        /// <summary>
        /// Resolves boundaries configured through the fluent pipeline builder.
        /// </summary>
        /// <returns>The execution boundaries configured for this pipeline.</returns>
        private IReadOnlyCollection<IPipelineExecutionBoundary<TRequest, TResponse>> CreatePipelineBoundaries()
            => _executionPlan is IExecutionBoundaryPlan<TRequest, TResponse> boundaryPlan
                ? boundaryPlan.CreateExecutionBoundaries(_serviceProvider)
                : Array.Empty<IPipelineExecutionBoundary<TRequest, TResponse>>();
    }
}
