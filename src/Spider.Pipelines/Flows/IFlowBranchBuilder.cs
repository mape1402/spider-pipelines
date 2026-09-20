namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Defines a builder for branches that converge to a common active value type.
    /// </summary>
    /// <typeparam name="TCurrent">The active value type used by branch conditions.</typeparam>
    /// <typeparam name="TNext">The convergence value type produced by each branch.</typeparam>
    public interface IFlowBranchBuilder<TCurrent, TNext>
    {
        /// <summary>
        /// Adds a conditional branch.
        /// </summary>
        /// <param name="condition">The branch condition.</param>
        /// <param name="configure">The branch flow configuration.</param>
        /// <returns>The current branch builder.</returns>
        IFlowBranchBuilder<TCurrent, TNext> When(
            Func<TCurrent, bool> condition,
            Action<IFlowBranchRouteBuilder<TCurrent, TNext>> configure);

        /// <summary>
        /// Adds the fallback branch.
        /// </summary>
        /// <param name="configure">The fallback branch flow configuration.</param>
        /// <returns>The current branch builder.</returns>
        IFlowBranchBuilder<TCurrent, TNext> Otherwise(
            Action<IFlowBranchRouteBuilder<TCurrent, TNext>> configure);
    }
}
