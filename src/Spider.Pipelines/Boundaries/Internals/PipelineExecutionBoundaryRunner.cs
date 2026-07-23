namespace Spider.Pipelines.Boundaries.Internals
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;

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
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RunAsync<TRequest>(
            IReadOnlyContext<TRequest> context,
            Func<Task> runCoreAsync,
            CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (runCoreAsync == null)
                throw new ArgumentNullException(nameof(runCoreAsync));

            var scopes = new Stack<IPipelineExecutionBoundaryScope>();
            var outcome = BoundaryOutcome.Pending();
            Exception terminalException = null;

            try
            {
                await BeginBoundariesAsync(scopes, CreateExecutionContext<TRequest>(context), cancellationToken);
                outcome = await RunCoreAndCaptureOutcomeAsync(context, runCoreAsync);
                terminalException = await TerminateBoundariesAndCaptureExceptionAsync(scopes, outcome, cancellationToken);
                ThrowIfTerminalException(terminalException);
                outcome.ThrowIfFaulted();
            }
            finally
            {
                await DisposeBoundariesAsync(scopes, outcome.Exception ?? terminalException);
            }
        }

        /// <summary>
        /// Executes request/response pipeline core logic inside registered boundaries.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object.</typeparam>
        /// <typeparam name="TResponse">The type of the response object.</typeparam>
        /// <param name="context">The current pipeline context.</param>
        /// <param name="runCoreAsync">The pipeline core operation to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the pipeline response.</returns>
        public async Task<TResponse> RunAsync<TRequest, TResponse>(
            IReadOnlyContext<TRequest, TResponse> context,
            Func<Task<TResponse>> runCoreAsync,
            CancellationToken cancellationToken)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (runCoreAsync == null)
                throw new ArgumentNullException(nameof(runCoreAsync));

            var scopes = new Stack<IPipelineExecutionBoundaryScope>();
            var outcome = BoundaryOutcome.Pending();
            var response = default(TResponse);
            Exception terminalException = null;

            try
            {
                await BeginBoundariesAsync(scopes, CreateExecutionContext<TRequest, TResponse>(context), cancellationToken);
                (outcome, response) = await RunCoreAndCaptureOutcomeAsync(context, runCoreAsync);
                terminalException = await TerminateBoundariesAndCaptureExceptionAsync(scopes, outcome, cancellationToken);
                ThrowIfTerminalException(terminalException);
                outcome.ThrowIfFaulted();
                return response;
            }
            finally
            {
                await DisposeBoundariesAsync(scopes, outcome.Exception ?? terminalException);
            }
        }

        /// <summary>
        /// Opens registered boundaries in registration order.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="context">The boundary execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task BeginBoundariesAsync(
            Stack<IPipelineExecutionBoundaryScope> scopes,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                foreach (var boundary in GetBoundaries())
                {
                    var scope = await boundary.BeginAsync(context, cancellationToken);
                    scopes.Push(scope ?? throw new InvalidOperationException("Pipeline execution boundary returned a null scope."));
                }
            }
            catch (Exception ex)
            {
                await FaultBoundariesPreservingOriginalAsync(scopes, ex, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Resolves registered boundaries, returning an empty collection when none are registered.
        /// </summary>
        /// <returns>The registered execution boundaries.</returns>
        private IEnumerable<IPipelineExecutionBoundary> GetBoundaries()
            => _serviceProvider.GetService(typeof(IEnumerable<IPipelineExecutionBoundary>)) is IEnumerable<IPipelineExecutionBoundary> boundaries
                ? boundaries
                : Array.Empty<IPipelineExecutionBoundary>();

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
        /// Applies the terminal boundary operation matching the pipeline outcome.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="outcome">The captured pipeline outcome.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static Task TerminateBoundariesAsync(
            Stack<IPipelineExecutionBoundaryScope> scopes,
            BoundaryOutcome outcome,
            CancellationToken cancellationToken)
            => outcome.State switch
            {
                BoundaryOutcomeState.Completed => CompleteBoundariesAsync(scopes, cancellationToken),
                BoundaryOutcomeState.Cancelled => CancelBoundariesAsync(scopes, cancellationToken),
                BoundaryOutcomeState.Faulted => FaultBoundariesAsync(scopes, outcome.Exception, cancellationToken),
                _ => Task.CompletedTask
            };

        /// <summary>
        /// Applies terminal boundary operations and captures boundary termination exceptions.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="outcome">The captured pipeline outcome.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The exception that should be surfaced from termination, or <c>null</c> when termination succeeds.</returns>
        private static async Task<Exception> TerminateBoundariesAndCaptureExceptionAsync(
            Stack<IPipelineExecutionBoundaryScope> scopes,
            BoundaryOutcome outcome,
            CancellationToken cancellationToken)
        {
            try
            {
                await TerminateBoundariesAsync(scopes, outcome, cancellationToken);
                return null;
            }
            catch (Exception ex)
            {
                return outcome.Exception ?? ex;
            }
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
        /// Completes opened scopes in reverse registration order.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task CompleteBoundariesAsync(
            IEnumerable<IPipelineExecutionBoundaryScope> scopes,
            CancellationToken cancellationToken)
        {
            foreach (var scope in scopes)
                await scope.CompleteAsync(cancellationToken);
        }

        /// <summary>
        /// Faults opened scopes in reverse registration order.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="exception">The exception that faulted the pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task FaultBoundariesAsync(
            IEnumerable<IPipelineExecutionBoundaryScope> scopes,
            Exception exception,
            CancellationToken cancellationToken)
        {
            foreach (var scope in scopes)
                await scope.FaultAsync(exception, cancellationToken);
        }

        /// <summary>
        /// Faults opened scopes while preserving the original begin exception.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="exception">The original begin exception.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task FaultBoundariesPreservingOriginalAsync(
            IEnumerable<IPipelineExecutionBoundaryScope> scopes,
            Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                await FaultBoundariesAsync(scopes, exception, cancellationToken);
            }
            catch
            {
                // The original begin exception must remain the surfaced failure.
            }
        }

        /// <summary>
        /// Cancels opened scopes in reverse registration order.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task CancelBoundariesAsync(
            IEnumerable<IPipelineExecutionBoundaryScope> scopes,
            CancellationToken cancellationToken)
        {
            foreach (var scope in scopes)
                await scope.CancelAsync(cancellationToken);
        }

        /// <summary>
        /// Disposes opened scopes in reverse registration order while preserving the original exception.
        /// </summary>
        /// <param name="scopes">The stack of opened scopes.</param>
        /// <param name="originalException">The original pipeline exception, when one exists.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task DisposeBoundariesAsync(
            Stack<IPipelineExecutionBoundaryScope> scopes,
            Exception originalException)
        {
            Exception disposeException = null;

            while (scopes.Count > 0)
            {
                try
                {
                    await scopes.Pop().DisposeAsync();
                }
                catch (Exception ex)
                {
                    disposeException ??= ex;
                }
            }

            if (originalException == null && disposeException != null)
                throw disposeException;
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
