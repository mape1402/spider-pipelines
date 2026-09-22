namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.RuntimeTracing;

    /// <summary>
    /// Executes composed flow step plans.
    /// </summary>
    internal static class FlowRunner
    {
        public static async Task<FlowExecutionState> RunAsync<TRequest>(
            string name,
            IReadOnlyDictionary<string, string> metadata,
            IReadOnlyList<IFlowStep> steps,
            TRequest request,
            ISpiderRuntimeTracer tracer,
            CancellationToken cancellationToken)
        {
            var state = new FlowExecutionState(typeof(TRequest), request, tracer);
            if (tracer == null || !tracer.IsEnabled)
            {
                await RunStepsAsync(steps, state, cancellationToken);
                return state;
            }

            var scope = await tracer.StartSpanAsync(new SpiderTraceSpanDefinition
            {
                ComponentKind = "spider.flow",
                DisplayName = name,
                Operation = "Flow",
                InputType = typeof(TRequest),
                StartedKind = SpiderTraceEventKind.FlowStarted,
                CompletedKind = SpiderTraceEventKind.FlowCompleted,
                FaultedKind = SpiderTraceEventKind.FlowFaulted,
                CancelledKind = SpiderTraceEventKind.FlowCancelled,
                Metadata = metadata ?? new Dictionary<string, string>()
            }, cancellationToken);

            try
            {
                await RunStepsAsync(steps, state, cancellationToken);
                await scope.CompleteAsync(cancellationToken);
                return state;
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

        public static async Task RunStepsAsync(
            IReadOnlyList<IFlowStep> steps,
            FlowExecutionState state,
            CancellationToken cancellationToken)
        {
            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (state.IsStopped)
                    break;

                await RunStepAsync(step, state, cancellationToken);
            }
        }

        private static async Task RunStepAsync(
            IFlowStep step,
            FlowExecutionState state,
            CancellationToken cancellationToken)
        {
            var tracer = state.Tracer;
            if (tracer == null || !tracer.IsEnabled)
            {
                await step.ExecuteAsync(state, cancellationToken);
                return;
            }

            var descriptor = step.Descriptor;
            var scope = await tracer.StartSpanAsync(new SpiderTraceSpanDefinition
            {
                ComponentKind = descriptor.ComponentKind,
                DisplayName = descriptor.DisplayName,
                Operation = descriptor.Operation,
                InputType = descriptor.InputType,
                OutputType = descriptor.OutputType,
                StartedKind = SpiderTraceEventKind.FlowStepStarted,
                CompletedKind = SpiderTraceEventKind.FlowStepCompleted,
                FaultedKind = SpiderTraceEventKind.FlowStepFaulted,
                CancelledKind = SpiderTraceEventKind.FlowCancelled
            }, cancellationToken);

            try
            {
                await step.ExecuteAsync(state, cancellationToken);
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
    }
}
