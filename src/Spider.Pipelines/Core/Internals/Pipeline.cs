namespace Spider.Pipelines.Core.Internals
{
    using Microsoft.Extensions.DependencyInjection;
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Boundaries.Internals;
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.RuntimeTracing;
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
            => await RunAsync(request, Array.Empty<IPipelineExecutionBoundary>(), cancellationToken);

        /// <inheritdoc/>
        public async Task RunAsync(TRequest request, Action<IExecutionBoundaryCollection> configureExecution, CancellationToken cancellationToken = default)
        {
            if (configureExecution == null)
                throw new ArgumentNullException(nameof(configureExecution));

            var executionBoundaries = CreateExecutionBoundaries(configureExecution);
            await RunAsync(request, executionBoundaries, cancellationToken);
        }

        /// <summary>
        /// Runs the pipeline with execution boundaries that have already been materialized.
        /// </summary>
        /// <param name="request">The request object to process.</param>
        /// <param name="executionBoundaries">The boundaries to apply to this materialized execution.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RunAsync(TRequest request, IReadOnlyCollection<IPipelineExecutionBoundary> executionBoundaries, CancellationToken cancellationToken = default)
        {
            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var context = new Context<TRequest>(request, _serviceProvider, cancellationToken);
            var boundaryRunner = new PipelineExecutionBoundaryRunner(_serviceProvider);

            await RunWithPipelineTraceAsync(
                typeof(TRequest),
                null,
                async () => await boundaryRunner.RunAsync(
                    context,
                    () => RunCoreAsync(context),
                    executionBoundaries,
                    cancellationToken),
                cancellationToken);
        }

        /// <summary>
        /// Runs the request-only pipeline core inside any registered execution boundaries.
        /// </summary>
        /// <param name="context">The current pipeline context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RunCoreAsync(Context<TRequest> context)
        {
            await RunStageAsync(
                "Pre-process",
                "Pipeline.PreProcess",
                typeof(TRequest),
                null,
                async () =>
                {
                    context.OnPreProcess();
                    await _executionPlan.OnPreProcessAsync(context);
                },
                context.CancellationToken);

            await RunStageAsync(
                "Target",
                "Pipeline.Target",
                typeof(TRequest),
                null,
                async () =>
                {
                    context.OnTargeting();
                    await _executionPlan.OnTargetingAsync(context, _targetHandler);
                },
                context.CancellationToken);

            await RunStageAsync(
                "Post-process",
                "Pipeline.PostProcess",
                typeof(TRequest),
                null,
                async () =>
                {
                    context.OnPostProcess();
                    await _executionPlan.OnPostProcessAsync(context);
                },
                context.CancellationToken);

            if (context.IsFailure())
                throw context.Exception;
        }

        /// <summary>
        /// Creates boundaries configured specifically for the current materialized execution.
        /// </summary>
        /// <param name="configureExecution">The action that configures execution-specific boundaries.</param>
        /// <returns>The execution-specific boundaries.</returns>
        private IReadOnlyCollection<IPipelineExecutionBoundary> CreateExecutionBoundaries(Action<IExecutionBoundaryCollection> configureExecution)
        {
            var boundaryCollection = new ExecutionBoundaryCollection();
            configureExecution(boundaryCollection);
            return boundaryCollection.CreateExecutionBoundaries(_serviceProvider);
        }

        private async Task RunWithPipelineTraceAsync(
            Type requestType,
            Type responseType,
            Func<Task> executeAsync,
            CancellationToken cancellationToken)
        {
            var tracer = GetTracer();
            if (tracer == null || !tracer.IsEnabled)
            {
                await executeAsync();
                return;
            }

            var scope = await tracer.StartSpanAsync(CreatePipelineDefinition(requestType, responseType), cancellationToken);
            try
            {
                await executeAsync();
                await scope.CompleteAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await scope.CancelAsync(cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await scope.FaultAsync(ex, cancellationToken);
                throw;
            }
        }

        private async Task RunStageAsync(
            string displayName,
            string operation,
            Type requestType,
            Type responseType,
            Func<Task> executeAsync,
            CancellationToken cancellationToken)
        {
            var tracer = GetTracer();
            if (tracer == null || !tracer.IsEnabled)
            {
                await executeAsync();
                return;
            }

            var scope = await tracer.StartSpanAsync(CreateStageDefinition(displayName, operation, requestType, responseType), cancellationToken);
            try
            {
                await executeAsync();
                await scope.CompleteAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await scope.CancelAsync(cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await scope.FaultAsync(ex, cancellationToken);
                throw;
            }
        }

        private ISpiderRuntimeTracer GetTracer()
            => _serviceProvider.GetService<ISpiderRuntimeTracer>();

        private static SpiderTraceSpanDefinition CreatePipelineDefinition(Type requestType, Type responseType)
            => new()
            {
                ComponentKind = "spider.pipeline",
                DisplayName = responseType == null
                    ? requestType.Name
                    : $"{requestType.Name} -> {responseType.Name}",
                Operation = "Pipeline",
                InputType = requestType,
                OutputType = responseType,
                StartedKind = SpiderTraceEventKind.PipelineStarted,
                CompletedKind = SpiderTraceEventKind.PipelineCompleted,
                FaultedKind = SpiderTraceEventKind.PipelineFaulted,
                CancelledKind = SpiderTraceEventKind.PipelineCancelled
            };

        private static SpiderTraceSpanDefinition CreateStageDefinition(
            string displayName,
            string operation,
            Type requestType,
            Type responseType)
            => new()
            {
                ComponentKind = "spider.pipeline-stage",
                DisplayName = displayName,
                Operation = operation,
                InputType = requestType,
                OutputType = responseType,
                StartedKind = SpiderTraceEventKind.PipelineStageStarted,
                CompletedKind = SpiderTraceEventKind.PipelineStageCompleted,
                FaultedKind = SpiderTraceEventKind.PipelineStageFaulted,
                CancelledKind = SpiderTraceEventKind.PipelineStageFaulted
            };
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
            => await RunAsync(request, Array.Empty<IPipelineExecutionBoundary>(), cancellationToken);

        /// <inheritdoc/>
        public async Task<TResponse> RunAsync(TRequest request, Action<IExecutionBoundaryCollection> configureExecution, CancellationToken cancellationToken = default)
        {
            if (configureExecution == null)
                throw new ArgumentNullException(nameof(configureExecution));

            var executionBoundaries = CreateExecutionBoundaries(configureExecution);
            return await RunAsync(request, executionBoundaries, cancellationToken);
        }

        /// <summary>
        /// Runs the pipeline with execution boundaries that have already been materialized.
        /// </summary>
        /// <param name="request">The request object to process.</param>
        /// <param name="executionBoundaries">The boundaries to apply to this materialized execution.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the pipeline response.</returns>
        private async Task<TResponse> RunAsync(TRequest request, IReadOnlyCollection<IPipelineExecutionBoundary> executionBoundaries, CancellationToken cancellationToken = default)
        {
            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var context = new Context<TRequest, TResponse>(request, _serviceProvider, cancellationToken);
            var boundaryRunner = new PipelineExecutionBoundaryRunner(_serviceProvider);

            return await RunWithPipelineTraceAsync(
                typeof(TRequest),
                typeof(TResponse),
                async () => await boundaryRunner.RunAsync(
                    context,
                    () => RunCoreAsync(context),
                    executionBoundaries,
                    cancellationToken),
                cancellationToken);
        }

        /// <summary>
        /// Runs the request/response pipeline core inside any registered execution boundaries.
        /// </summary>
        /// <param name="context">The current pipeline context.</param>
        /// <returns>A task containing the pipeline response.</returns>
        private async Task<TResponse> RunCoreAsync(Context<TRequest, TResponse> context)
        {
            await RunStageAsync(
                "Pre-process",
                "Pipeline.PreProcess",
                typeof(TRequest),
                typeof(TResponse),
                async () =>
                {
                    context.OnPreProcess();
                    await _executionPlan.OnPreProcessAsync(context);
                },
                context.CancellationToken);

            TResponse response = default;
            await RunStageAsync(
                "Target",
                "Pipeline.Target",
                typeof(TRequest),
                typeof(TResponse),
                async () =>
                {
                    context.OnTargeting();
                    response = await _executionPlan.OnTargetingAsync(context, _targetHandler);
                },
                context.CancellationToken);

            await RunStageAsync(
                "Post-process",
                "Pipeline.PostProcess",
                typeof(TRequest),
                typeof(TResponse),
                async () =>
                {
                    context.OnPostProcess();
                    await _executionPlan.OnPostProcessAsync(context);
                },
                context.CancellationToken);

            if (context.IsFailure())
                throw context.Exception;

            return response;
        }

        /// <summary>
        /// Creates boundaries configured specifically for the current materialized execution.
        /// </summary>
        /// <param name="configureExecution">The action that configures execution-specific boundaries.</param>
        /// <returns>The execution-specific boundaries.</returns>
        private IReadOnlyCollection<IPipelineExecutionBoundary> CreateExecutionBoundaries(Action<IExecutionBoundaryCollection> configureExecution)
        {
            var boundaryCollection = new ExecutionBoundaryCollection();
            configureExecution(boundaryCollection);
            return boundaryCollection.CreateExecutionBoundaries(_serviceProvider);
        }

        private async Task<TResponse> RunWithPipelineTraceAsync(
            Type requestType,
            Type responseType,
            Func<Task<TResponse>> executeAsync,
            CancellationToken cancellationToken)
        {
            var tracer = GetTracer();
            if (tracer == null || !tracer.IsEnabled)
                return await executeAsync();

            var scope = await tracer.StartSpanAsync(CreatePipelineDefinition(requestType, responseType), cancellationToken);
            try
            {
                var response = await executeAsync();
                await scope.CompleteAsync(cancellationToken);
                return response;
            }
            catch (OperationCanceledException)
            {
                await scope.CancelAsync(cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await scope.FaultAsync(ex, cancellationToken);
                throw;
            }
        }

        private async Task RunStageAsync(
            string displayName,
            string operation,
            Type requestType,
            Type responseType,
            Func<Task> executeAsync,
            CancellationToken cancellationToken)
        {
            var tracer = GetTracer();
            if (tracer == null || !tracer.IsEnabled)
            {
                await executeAsync();
                return;
            }

            var scope = await tracer.StartSpanAsync(CreateStageDefinition(displayName, operation, requestType, responseType), cancellationToken);
            try
            {
                await executeAsync();
                await scope.CompleteAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await scope.CancelAsync(cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await scope.FaultAsync(ex, cancellationToken);
                throw;
            }
        }

        private ISpiderRuntimeTracer GetTracer()
            => _serviceProvider.GetService<ISpiderRuntimeTracer>();

        private static SpiderTraceSpanDefinition CreatePipelineDefinition(Type requestType, Type responseType)
            => new()
            {
                ComponentKind = "spider.pipeline",
                DisplayName = responseType == null
                    ? requestType.Name
                    : $"{requestType.Name} -> {responseType.Name}",
                Operation = "Pipeline",
                InputType = requestType,
                OutputType = responseType,
                StartedKind = SpiderTraceEventKind.PipelineStarted,
                CompletedKind = SpiderTraceEventKind.PipelineCompleted,
                FaultedKind = SpiderTraceEventKind.PipelineFaulted,
                CancelledKind = SpiderTraceEventKind.PipelineCancelled
            };

        private static SpiderTraceSpanDefinition CreateStageDefinition(
            string displayName,
            string operation,
            Type requestType,
            Type responseType)
            => new()
            {
                ComponentKind = "spider.pipeline-stage",
                DisplayName = displayName,
                Operation = operation,
                InputType = requestType,
                OutputType = responseType,
                StartedKind = SpiderTraceEventKind.PipelineStageStarted,
                CompletedKind = SpiderTraceEventKind.PipelineStageCompleted,
                FaultedKind = SpiderTraceEventKind.PipelineStageFaulted,
                CancelledKind = SpiderTraceEventKind.PipelineStageFaulted
            };
    }
}
