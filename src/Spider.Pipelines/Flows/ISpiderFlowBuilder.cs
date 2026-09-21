namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Defines a builder for a composed flow that does not require a response value.
    /// </summary>
    /// <typeparam name="TRequest">The request type that starts the flow.</typeparam>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    public interface ISpiderFlowBuilder<TRequest, TCurrent>
    {
        /// <summary>
        /// Selects a runtime profile for the flow.
        /// </summary>
        /// <param name="profileName">The runtime profile name.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> UsingProfile(string profileName);

        /// <summary>
        /// Assigns a short description to the flow.
        /// </summary>
        /// <param name="description">The description to show in generated documentation.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Describe(string description);

        /// <summary>
        /// Adds descriptive tags to the flow.
        /// </summary>
        /// <param name="tags">The tags to show in generated documentation.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Tags(params string[] tags);

        /// <summary>
        /// Adds custom key-value metadata to the flow.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Metadata(string key, string value);

        /// <summary>
        /// Adds the next step to the flow.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, TNext> step);

        /// <summary>
        /// Adds the next step to the flow with descriptive metadata.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, TNext> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next asynchronous step to the flow.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The asynchronous step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step);

        /// <summary>
        /// Adds the next asynchronous step to the flow with descriptive metadata.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The asynchronous step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step to the flow.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Then(Func<TCurrent, CancellationToken, Task> step);

        /// <summary>
        /// Adds the next side-effect step to the flow with descriptive metadata.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Then(Func<TCurrent, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step to the flow.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent> step);

        /// <summary>
        /// Adds the next side-effect step to the flow with descriptive metadata.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step to the flow.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent, CancellationToken> step);

        /// <summary>
        /// Adds the next side-effect step to the flow with descriptive metadata.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> Then(Action<TCurrent, CancellationToken> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next step using a specific value from the flow history.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue, TNext>(Func<TValue, TNext> step);

        /// <summary>
        /// Adds the next step using a specific value from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue, TNext>(Func<TValue, TNext> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step using a specific value from the flow history.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step);

        /// <summary>
        /// Adds the next side-effect step using a specific value from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next step using two specific values from the flow history.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step);

        /// <summary>
        /// Adds the next step using two specific values from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step using two specific values from the flow history.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step);

        /// <summary>
        /// Adds the next side-effect step using two specific values from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-stop outcome to apply when the condition is false.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowStop otherwise);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue with descriptive metadata.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-stop outcome to apply when the condition is false.</param>
        /// <param name="configure">The condition metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowStop otherwise, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-fault outcome to apply when the condition is false.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue with descriptive metadata.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-fault outcome to apply when the condition is false.</param>
        /// <param name="configure">The condition metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds a branch that converges into a new active value type.
        /// </summary>
        /// <typeparam name="TNext">The branch convergence type.</typeparam>
        /// <param name="configure">The branch configuration.</param>
        /// <returns>A flow builder with the branch convergence type.</returns>
        ISpiderFlowBuilder<TRequest, TNext> Branch<TNext>(Action<IFlowBranchBuilder<TCurrent, TNext>> configure);

        /// <summary>
        /// Executes the composed flow.
        /// </summary>
        /// <param name="request">The request value.</param>
        /// <param name="cancellationToken">The cancellation token for the execution.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunAsync(TRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a builder for a composed flow that must produce a response value.
    /// </summary>
    /// <typeparam name="TRequest">The request type that starts the flow.</typeparam>
    /// <typeparam name="TCurrent">The current active value type.</typeparam>
    /// <typeparam name="TResponse">The response type the flow must produce.</typeparam>
    public interface ISpiderFlowBuilder<TRequest, TCurrent, TResponse>
    {
        /// <summary>
        /// Selects a runtime profile for the flow.
        /// </summary>
        /// <param name="profileName">The runtime profile name.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> UsingProfile(string profileName);

        /// <summary>
        /// Assigns a short description to the flow.
        /// </summary>
        /// <param name="description">The description to show in generated documentation.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Describe(string description);

        /// <summary>
        /// Adds descriptive tags to the flow.
        /// </summary>
        /// <param name="tags">The tags to show in generated documentation.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Tags(params string[] tags);

        /// <summary>
        /// Adds custom key-value metadata to the flow.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Metadata(string key, string value);

        /// <summary>
        /// Adds the next step to the flow.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, TNext> step);

        /// <summary>
        /// Adds the next step to the flow with descriptive metadata.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, TNext> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next asynchronous step to the flow.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The asynchronous step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step);

        /// <summary>
        /// Adds the next asynchronous step to the flow with descriptive metadata.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The asynchronous step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step to the flow.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Func<TCurrent, CancellationToken, Task> step);

        /// <summary>
        /// Adds the next side-effect step to the flow with descriptive metadata.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Func<TCurrent, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step to the flow.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent> step);

        /// <summary>
        /// Adds the next side-effect step to the flow with descriptive metadata.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step to the flow.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent, CancellationToken> step);

        /// <summary>
        /// Adds the next side-effect step to the flow with descriptive metadata.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> Then(Action<TCurrent, CancellationToken> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next step using a specific value from the flow history.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue, TNext>(Func<TValue, TNext> step);

        /// <summary>
        /// Adds the next step using a specific value from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue, TNext>(Func<TValue, TNext> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step using a specific value from the flow history.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step);

        /// <summary>
        /// Adds the next side-effect step using a specific value from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue>(Func<TValue, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next step using two specific values from the flow history.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step);

        /// <summary>
        /// Adds the next step using two specific values from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>A flow builder with the next active value type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> ThenWith<TValue1, TValue2, TNext>(Func<TValue1, TValue2, TNext> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds the next side-effect step using two specific values from the flow history.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step);

        /// <summary>
        /// Adds the next side-effect step using two specific values from the flow history with descriptive metadata.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <param name="configure">The step metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-return outcome to apply when the condition is false.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowReturn<TCurrent, TResponse> otherwise);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue with descriptive metadata.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-return outcome to apply when the condition is false.</param>
        /// <param name="configure">The condition metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowReturn<TCurrent, TResponse> otherwise, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-fault outcome to apply when the condition is false.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise);

        /// <summary>
        /// Adds a condition that must be true for the flow to continue with descriptive metadata.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="otherwise">The early-fault outcome to apply when the condition is false.</param>
        /// <param name="configure">The condition metadata configuration.</param>
        /// <returns>The current flow builder.</returns>
        ISpiderFlowBuilder<TRequest, TCurrent, TResponse> ContinueIf(Func<TCurrent, bool> condition, FlowThrow otherwise, Action<IFlowMetadataBuilder> configure);

        /// <summary>
        /// Adds a branch that converges into a new active value type.
        /// </summary>
        /// <typeparam name="TNext">The branch convergence type.</typeparam>
        /// <param name="configure">The branch configuration.</param>
        /// <returns>A flow builder with the branch convergence type.</returns>
        ISpiderFlowBuilder<TRequest, TNext, TResponse> Branch<TNext>(Action<IFlowBranchBuilder<TCurrent, TNext>> configure);

        /// <summary>
        /// Executes the composed flow and returns the response.
        /// </summary>
        /// <param name="request">The request value.</param>
        /// <param name="cancellationToken">The cancellation token for the execution.</param>
        /// <returns>The response produced by the flow.</returns>
        Task<TResponse> RunAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
