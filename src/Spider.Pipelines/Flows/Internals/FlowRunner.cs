namespace Spider.Pipelines.Flows.Internals
{
    /// <summary>
    /// Executes composed flow step plans.
    /// </summary>
    internal static class FlowRunner
    {
        public static async Task<FlowExecutionState> RunAsync<TRequest>(
            IReadOnlyList<IFlowStep> steps,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var state = new FlowExecutionState(typeof(TRequest), request);

            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (state.IsStopped)
                    break;

                await step.ExecuteAsync(state, cancellationToken);
            }

            return state;
        }
    }
}
