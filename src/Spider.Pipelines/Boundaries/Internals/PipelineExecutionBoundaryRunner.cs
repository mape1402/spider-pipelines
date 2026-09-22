namespace Spider.Pipelines.Boundaries.Internals
{
    using Microsoft.Extensions.DependencyInjection;
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.RuntimeTracing;

    /// <summary>
    /// Runs registered execution boundaries around a complete pipeline execution.
    /// </summary>
    internal sealed class PipelineExecutionBoundaryRunner
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecutionBoundaryRunner"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve registered boundaries.</param>
        public PipelineExecutionBoundaryRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Executes request-only pipeline core logic inside registered boundaries.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="context">The current pipeline context.</param>
        /// <param name="runCoreAsync">The pipeline core operation to execute.</param>
        /// <param name="executionBoundaries">The execution boundaries to append after globally registered boundaries.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RunAsync<TRequest>(
            IReadOnlyContext<TRequest> context,
            Func<Task> runCoreAsync,
            IEnumerable<IPipelineExecutionBoundary> executionBoundaries,
            CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (runCoreAsync == null)
                throw new ArgumentNullException(nameof(runCoreAsync));

            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var executionContext = CreateExecutionContext<TRequest>(context);
            var boundaries = ResolveBoundaries(executionBoundaries);
            var openedBoundaries = await BeginBoundariesAsync(boundaries, executionContext, cancellationToken);
            var outcome = await RunCoreAndCaptureOutcomeAsync(context, runCoreAsync);
            var terminalException = await TerminateBoundariesAndCaptureExceptionAsync(openedBoundaries, executionContext, outcome, cancellationToken);

            ThrowIfTerminalException(terminalException);
            outcome.ThrowIfFaulted();
        }

        /// <summary>
        /// Executes request/response pipeline core logic inside registered boundaries.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="context">The current pipeline context.</param>
        /// <param name="runCoreAsync">The pipeline core operation to execute.</param>
        /// <param name="executionBoundaries">The execution boundaries to append after globally registered boundaries.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the pipeline response.</returns>
        public async Task<TResponse> RunAsync<TRequest, TResponse>(
            IReadOnlyContext<TRequest, TResponse> context,
            Func<Task<TResponse>> runCoreAsync,
            IEnumerable<IPipelineExecutionBoundary> executionBoundaries,
            CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (runCoreAsync == null)
                throw new ArgumentNullException(nameof(runCoreAsync));

            if (executionBoundaries == null)
                throw new ArgumentNullException(nameof(executionBoundaries));

            var executionContext = CreateExecutionContext<TRequest, TResponse>(context);
            var boundaries = ResolveBoundaries(executionBoundaries);
            var openedBoundaries = await BeginBoundariesAsync(boundaries, executionContext, cancellationToken);
            var (outcome, response) = await RunCoreAndCaptureOutcomeAsync(context, runCoreAsync);
            var terminalException = await TerminateBoundariesAndCaptureExceptionAsync(openedBoundaries, executionContext, outcome, cancellationToken);

            ThrowIfTerminalException(terminalException);
            outcome.ThrowIfFaulted();
            return response;
        }

        /// <summary>
        /// Opens registered boundaries in registration order.
        /// </summary>
        /// <param name="boundaries">The registered execution boundaries.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The boundaries that opened successfully.</returns>
        private async Task<IReadOnlyCollection<IPipelineExecutionBoundary>> BeginBoundariesAsync(
            IReadOnlyCollection<IPipelineExecutionBoundary> boundaries,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var openedBoundaries = new List<IPipelineExecutionBoundary>();

            try
            {
                foreach (var boundary in boundaries)
                {
                    await TraceBoundaryOperationAsync(
                        boundary,
                        "Boundary.Begin",
                        SpiderTraceEventKind.BoundaryBeginStarted,
                        SpiderTraceEventKind.BoundaryBeginCompleted,
                        async () => await boundary.BeginAsync(context, cancellationToken),
                        context,
                        cancellationToken);
                    openedBoundaries.Add(boundary);
                }

                return openedBoundaries;
            }
            catch (Exception ex)
            {
                await FaultBoundariesPreservingOriginalAsync(openedBoundaries, context, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Runs request-only pipeline core logic and captures its terminal outcome.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="context">The current pipeline context.</param>
        /// <param name="runCoreAsync">The pipeline core operation to execute.</param>
        /// <returns>The captured boundary outcome.</returns>
        private static async Task<BoundaryOutcome> RunCoreAndCaptureOutcomeAsync<TRequest>(
            IReadOnlyContext<TRequest> context,
            Func<Task> runCoreAsync)
        {
            try
            {
                await runCoreAsync();
                return context.IsCancelled() ? BoundaryOutcome.Cancelled() : BoundaryOutcome.Completed();
            }
            catch (Exception ex)
            {
                return BoundaryOutcome.Faulted(ex);
            }
        }

        /// <summary>
        /// Runs request/response pipeline core logic and captures its terminal outcome.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="context">The current pipeline context.</param>
        /// <param name="runCoreAsync">The pipeline core operation to execute.</param>
        /// <returns>The captured boundary outcome and response.</returns>
        private static async Task<(BoundaryOutcome Outcome, TResponse Response)> RunCoreAndCaptureOutcomeAsync<TRequest, TResponse>(
            IReadOnlyContext<TRequest, TResponse> context,
            Func<Task<TResponse>> runCoreAsync)
        {
            try
            {
                var response = await runCoreAsync();
                var outcome = context.IsCancelled() ? BoundaryOutcome.Cancelled() : BoundaryOutcome.Completed();
                return (outcome, response);
            }
            catch (Exception ex)
            {
                return (BoundaryOutcome.Faulted(ex), default);
            }
        }

        /// <summary>
        /// Applies terminal boundary operations and captures boundary termination exceptions.
        /// </summary>
        /// <param name="boundaries">The boundaries that opened successfully.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="outcome">The captured pipeline outcome.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The exception that should be surfaced from termination, or <c>null</c> when termination succeeds.</returns>
        private async Task<Exception> TerminateBoundariesAndCaptureExceptionAsync(
            IReadOnlyCollection<IPipelineExecutionBoundary> boundaries,
            PipelineExecutionContext context,
            BoundaryOutcome outcome,
            CancellationToken cancellationToken)
        {
            try
            {
                await TerminateBoundariesAsync(boundaries, context, outcome, cancellationToken);
                return null;
            }
            catch (Exception ex)
            {
                return outcome.Exception ?? ex;
            }
        }

        /// <summary>
        /// Applies the terminal boundary operation matching the pipeline outcome.
        /// </summary>
        /// <param name="boundaries">The boundaries that opened successfully.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="outcome">The captured pipeline outcome.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task TerminateBoundariesAsync(
            IReadOnlyCollection<IPipelineExecutionBoundary> boundaries,
            PipelineExecutionContext context,
            BoundaryOutcome outcome,
            CancellationToken cancellationToken)
            => outcome.State switch
            {
                BoundaryOutcomeState.Completed => CompleteBoundariesAsync(boundaries, context, cancellationToken),
                BoundaryOutcomeState.Cancelled => CancelBoundariesAsync(boundaries, context, cancellationToken),
                BoundaryOutcomeState.Faulted => FaultBoundariesAsync(boundaries, context, outcome.Exception, cancellationToken),
                _ => Task.CompletedTask
            };

        /// <summary>
        /// Completes opened boundaries in reverse registration order.
        /// </summary>
        /// <param name="boundaries">The boundaries that opened successfully.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CompleteBoundariesAsync(
            IEnumerable<IPipelineExecutionBoundary> boundaries,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            foreach (var boundary in boundaries.Reverse())
                await TraceBoundaryOperationAsync(
                    boundary,
                    "Boundary.Complete",
                    SpiderTraceEventKind.BoundaryCompleteStarted,
                    SpiderTraceEventKind.BoundaryCompleteCompleted,
                    async () => await boundary.CompleteAsync(context, cancellationToken),
                    context,
                    cancellationToken);
        }

        /// <summary>
        /// Faults opened boundaries in reverse registration order.
        /// </summary>
        /// <param name="boundaries">The boundaries that opened successfully.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="exception">The exception that faulted the pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task FaultBoundariesAsync(
            IEnumerable<IPipelineExecutionBoundary> boundaries,
            PipelineExecutionContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            foreach (var boundary in boundaries.Reverse())
                await TraceBoundaryOperationAsync(
                    boundary,
                    "Boundary.Fault",
                    SpiderTraceEventKind.BoundaryFaultStarted,
                    SpiderTraceEventKind.BoundaryFaultCompleted,
                    async () => await boundary.FaultAsync(context, exception, cancellationToken),
                    context,
                    cancellationToken);
        }

        /// <summary>
        /// Faults opened boundaries while preserving the original begin exception.
        /// </summary>
        /// <param name="boundaries">The boundaries that opened successfully.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="exception">The original begin exception.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task FaultBoundariesPreservingOriginalAsync(
            IEnumerable<IPipelineExecutionBoundary> boundaries,
            PipelineExecutionContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                await FaultBoundariesAsync(boundaries, context, exception, cancellationToken);
            }
            catch
            {
                // The original begin exception must remain the surfaced failure.
            }
        }

        /// <summary>
        /// Cancels opened boundaries in reverse registration order.
        /// </summary>
        /// <param name="boundaries">The boundaries that opened successfully.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CancelBoundariesAsync(
            IEnumerable<IPipelineExecutionBoundary> boundaries,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            foreach (var boundary in boundaries.Reverse())
                await TraceBoundaryOperationAsync(
                    boundary,
                    "Boundary.Cancel",
                    SpiderTraceEventKind.BoundaryCancelStarted,
                    SpiderTraceEventKind.BoundaryCancelCompleted,
                    async () => await boundary.CancelAsync(context, cancellationToken),
                    context,
                    cancellationToken);
        }

        /// <summary>
        /// Throws an exception captured during boundary termination.
        /// </summary>
        /// <param name="exception">The captured boundary termination exception.</param>
        private static void ThrowIfTerminalException(Exception exception)
        {
            if (exception != null)
                throw exception;
        }

        /// <summary>
        /// Executes a boundary operation inside a runtime trace span when tracing is enabled.
        /// </summary>
        /// <param name="boundary">The boundary being executed.</param>
        /// <param name="operation">The boundary operation name.</param>
        /// <param name="startedKind">The started event kind.</param>
        /// <param name="completedKind">The completed event kind.</param>
        /// <param name="executeAsync">The boundary operation to execute.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task TraceBoundaryOperationAsync(
            IPipelineExecutionBoundary boundary,
            string operation,
            SpiderTraceEventKind startedKind,
            SpiderTraceEventKind completedKind,
            Func<Task> executeAsync,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var tracer = _serviceProvider.GetService<ISpiderRuntimeTracer>();
            if (tracer == null || !tracer.IsEnabled)
            {
                await executeAsync();
                return;
            }

            var boundaryType = boundary.GetType();
            var scope = await tracer.StartSpanAsync(new SpiderTraceSpanDefinition
            {
                ComponentKind = "spider.boundary",
                DisplayName = boundaryType.Name,
                Operation = operation,
                InputType = context.RequestType,
                OutputType = context.ResponseType,
                StartedKind = startedKind,
                CompletedKind = completedKind,
                FaultedKind = completedKind,
                CancelledKind = completedKind,
                Metadata = new Dictionary<string, string>
                {
                    ["boundaryType"] = boundaryType.FullName
                }
            }, cancellationToken);

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

        /// <summary>
        /// Resolves registered boundaries and appends execution-specific boundaries.
        /// </summary>
        /// <param name="executionBoundaries">The execution-specific boundaries to append after globally registered boundaries.</param>
        /// <returns>The registered execution boundaries.</returns>
        private IReadOnlyCollection<IPipelineExecutionBoundary> ResolveBoundaries(IEnumerable<IPipelineExecutionBoundary> executionBoundaries)
        {
            var globalBoundaries = _serviceProvider.GetService(typeof(IEnumerable<IPipelineExecutionBoundary>)) is IEnumerable<IPipelineExecutionBoundary> boundaries
                ? boundaries
                : Array.Empty<IPipelineExecutionBoundary>();

            return globalBoundaries.Concat(executionBoundaries).ToArray();
        }

        /// <summary>
        /// Creates boundary metadata for a request-only pipeline.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <param name="context">The current pipeline context.</param>
        /// <returns>The boundary execution context.</returns>
        private static PipelineExecutionContext CreateExecutionContext<TRequest>(IReadOnlyContext<TRequest> context)
            => new PipelineExecutionContext
            {
                RequestType = typeof(TRequest),
                ResponseType = null,
                Request = context.Request,
                Services = context.Services
            };

        /// <summary>
        /// Creates boundary metadata for a request/response pipeline.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="context">The current pipeline context.</param>
        /// <returns>The boundary execution context.</returns>
        private static PipelineExecutionContext CreateExecutionContext<TRequest, TResponse>(IReadOnlyContext<TRequest, TResponse> context)
            => new PipelineExecutionContext
            {
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                Request = context.Request,
                Services = context.Services
            };
    }
}
