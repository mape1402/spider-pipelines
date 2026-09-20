namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Defines a builder for a single branch route.
    /// </summary>
    /// <typeparam name="TCurrent">The current active value type for the branch route.</typeparam>
    /// <typeparam name="TBranchResult">The result type every route must converge to.</typeparam>
    public interface IFlowBranchRouteBuilder<TCurrent, TBranchResult>
    {
        /// <summary>
        /// Adds the next step to the branch route.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A branch route builder with the next active value type.</returns>
        IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, TNext> step);

        /// <summary>
        /// Adds the next asynchronous step to the branch route.
        /// </summary>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The asynchronous step delegate.</param>
        /// <returns>A branch route builder with the next active value type.</returns>
        IFlowBranchRouteBuilder<TNext, TBranchResult> Then<TNext>(Func<TCurrent, CancellationToken, Task<TNext>> step);

        /// <summary>
        /// Adds the next side-effect step to the branch route.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current branch route builder.</returns>
        IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Func<TCurrent, CancellationToken, Task> step);

        /// <summary>
        /// Adds the next side-effect step to the branch route.
        /// </summary>
        /// <param name="step">The side-effect step delegate.</param>
        /// <returns>The current branch route builder.</returns>
        IFlowBranchRouteBuilder<TCurrent, TBranchResult> Then(Action<TCurrent> step);

        /// <summary>
        /// Adds the next step using a specific value from the flow history.
        /// </summary>
        /// <typeparam name="TValue">The history value type used by the step.</typeparam>
        /// <typeparam name="TNext">The next active value type.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>A branch route builder with the next active value type.</returns>
        IFlowBranchRouteBuilder<TNext, TBranchResult> ThenWith<TValue, TNext>(Func<TValue, TNext> step);

        /// <summary>
        /// Adds the next side-effect step using two specific values from the flow history.
        /// </summary>
        /// <typeparam name="TValue1">The first history value type used by the step.</typeparam>
        /// <typeparam name="TValue2">The second history value type used by the step.</typeparam>
        /// <param name="step">The step delegate.</param>
        /// <returns>The current branch route builder.</returns>
        IFlowBranchRouteBuilder<TCurrent, TBranchResult> ThenWith<TValue1, TValue2>(Func<TValue1, TValue2, CancellationToken, Task> step);
    }
}
