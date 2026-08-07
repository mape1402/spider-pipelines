using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Testing.Internals;

namespace Spider.Testing
{
    /// <summary>
    /// Executes requests through Spider testing instrumentation.
    /// </summary>
    public sealed class SpiderTestingHost : ISpiderTesting
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SpiderTestingRegistry _registry;
        private readonly Dictionary<Type, Exception> _boundaryFailures;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderTestingHost"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve handlers and boundaries.</param>
        /// <param name="registry">The discovered testing registry.</param>
        internal SpiderTestingHost(IServiceProvider serviceProvider, SpiderTestingRegistry registry)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _boundaryFailures = new Dictionary<Type, Exception>();
            Trace = new ExecutionTrace();
        }

        /// <inheritdoc/>
        public ExecutionTrace Trace { get; }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Trace.Clear();

            var requestType = request.GetType();
            var handler = FindHandler(requestType);
            var metadata = CreateMetadata(request, handler);
            var context = new PipelineExecutionContext
            {
                Request = request,
                RequestType = requestType,
                ResponseType = handler.ResponseType,
                Services = _serviceProvider
            };

            Trace.Add("RequestMetadata", "Record", requestType, metadata: metadata);
            Trace.Add("PipelineExecution", "Begin", requestType, metadata: metadata);

            var openedBoundaries = await BeginBoundariesAsync(context, cancellationToken);

            try
            {
                var result = await InvokeHandlerAsync(handler, request, cancellationToken);
                await CompleteBoundariesAsync(openedBoundaries, context, cancellationToken);
                Trace.Add("PipelineExecution", "Complete", requestType, metadata: metadata);
                return result;
            }
            catch (OperationCanceledException ex)
            {
                await CancelBoundariesAsync(openedBoundaries, context, cancellationToken);
                Trace.Add("PipelineExecution", "Cancel", requestType, ex, metadata);
                throw;
            }
            catch (Exception ex)
            {
                await FaultBoundariesAsync(openedBoundaries, context, ex, cancellationToken);
                Trace.Add("PipelineExecution", "Fault", requestType, ex, metadata);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TResponse> ExecuteAsync<TResponse>(object request, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteAsync(request, cancellationToken);
            return result == null ? default : (TResponse)result;
        }

        /// <inheritdoc/>
        public void FailInside<TBoundary>(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            _boundaryFailures[typeof(TBoundary)] = exception;
        }

        private HandlerDescriptor FindHandler(Type requestType)
        {
            var handlers = _registry.Handlers
                .Where(handler => handler.RequestType.IsAssignableFrom(requestType))
                .OrderByDescending(handler => handler.RequestType == requestType)
                .ToArray();

            if (handlers.Length == 0)
                throw new InvalidOperationException($"No Spider testing handler was discovered for request type '{requestType.FullName}'.");

            var bestHandlers = handlers
                .Where(handler => handler.RequestType == handlers[0].RequestType)
                .ToArray();

            if (bestHandlers.Length > 1)
                throw new InvalidOperationException($"Multiple Spider testing handlers were discovered for request type '{requestType.FullName}'.");

            return bestHandlers[0];
        }

        private async Task<IReadOnlyList<(BoundaryDescriptor Descriptor, IPipelineExecutionBoundary Boundary)>> BeginBoundariesAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var openedBoundaries = new List<(BoundaryDescriptor Descriptor, IPipelineExecutionBoundary Boundary)>();

            foreach (var descriptor in _registry.Boundaries)
            {
                var boundary = (IPipelineExecutionBoundary)_serviceProvider.GetRequiredService(descriptor.BoundaryType);

                try
                {
                    Trace.Add(descriptor.Name, "Begin", context.RequestType);
                    ThrowIfSimulatedBoundaryFailure(descriptor);
                    await boundary.BeginAsync(context, cancellationToken);
                    openedBoundaries.Add((descriptor, boundary));
                }
                catch (Exception ex)
                {
                    Trace.Add(descriptor.Name, "Fault", context.RequestType, ex);
                    await FaultBoundariesAsync(openedBoundaries, context, ex, cancellationToken);
                    Trace.Add("PipelineExecution", "Fault", context.RequestType, ex);
                    throw;
                }
            }

            return openedBoundaries;
        }

        private async Task<object> InvokeHandlerAsync(
            HandlerDescriptor handler,
            object request,
            CancellationToken cancellationToken)
        {
            var service = _serviceProvider.GetRequiredService(handler.ServiceType);

            try
            {
                Trace.Add("HandlerExecution", "Begin", request.GetType());
                var result = await HandlerInvoker.InvokeAsync(handler, service, request, cancellationToken);
                Trace.Add("HandlerExecution", "Complete", request.GetType());
                return result;
            }
            catch (Exception ex)
            {
                Trace.Add("HandlerExecution", "Fault", request.GetType(), ex);
                throw;
            }
        }

        private async Task CompleteBoundariesAsync(
            IEnumerable<(BoundaryDescriptor Descriptor, IPipelineExecutionBoundary Boundary)> boundaries,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            foreach (var item in boundaries.Reverse())
            {
                Trace.Add(item.Descriptor.Name, GetCompleteOperation(item.Descriptor), context.RequestType);
                await item.Boundary.CompleteAsync(context, cancellationToken);
            }
        }

        private async Task FaultBoundariesAsync(
            IEnumerable<(BoundaryDescriptor Descriptor, IPipelineExecutionBoundary Boundary)> boundaries,
            PipelineExecutionContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            foreach (var item in boundaries.Reverse())
            {
                Trace.Add(item.Descriptor.Name, GetFaultOperation(item.Descriptor), context.RequestType, exception);
                await item.Boundary.FaultAsync(context, exception, cancellationToken);
            }
        }

        private async Task CancelBoundariesAsync(
            IEnumerable<(BoundaryDescriptor Descriptor, IPipelineExecutionBoundary Boundary)> boundaries,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            foreach (var item in boundaries.Reverse())
            {
                Trace.Add(item.Descriptor.Name, GetCancelOperation(item.Descriptor), context.RequestType);
                await item.Boundary.CancelAsync(context, cancellationToken);
            }
        }

        private void ThrowIfSimulatedBoundaryFailure(BoundaryDescriptor descriptor)
        {
            if (_boundaryFailures.TryGetValue(descriptor.BoundaryType, out var exception))
                throw exception;
        }

        private static IReadOnlyDictionary<string, object> CreateMetadata(object request, HandlerDescriptor handler)
            => new Dictionary<string, object>
            {
                ["Request"] = request,
                ["RequestType"] = request.GetType(),
                ["HandlerType"] = handler.ServiceType,
                ["HandlerMethod"] = handler.Method.Name,
                ["ResponseType"] = handler.ResponseType
            };

        private static string GetCompleteOperation(BoundaryDescriptor descriptor)
            => IsTransactionBoundary(descriptor) ? "Commit" : "Complete";

        private static string GetFaultOperation(BoundaryDescriptor descriptor)
            => IsTransactionBoundary(descriptor) ? "Rollback" : "Fault";

        private static string GetCancelOperation(BoundaryDescriptor descriptor)
            => IsTransactionBoundary(descriptor) ? "Rollback" : "Cancel";

        private static bool IsTransactionBoundary(BoundaryDescriptor descriptor)
            => descriptor.Name.IndexOf("Transaction", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
