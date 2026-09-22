namespace Spider.Pipelines.Flows.Internals
{
    /// <summary>
    /// Defines a runtime step in a composed flow.
    /// </summary>
    internal interface IFlowStep
    {
        /// <summary>
        /// Gets the runtime tracing descriptor for the step.
        /// </summary>
        FlowStepDescriptor Descriptor { get; }

        /// <summary>
        /// Executes the step.
        /// </summary>
        /// <param name="state">The flow execution state.</param>
        /// <param name="cancellationToken">The cancellation token for the execution.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(FlowExecutionState state, CancellationToken cancellationToken);
    }
}
