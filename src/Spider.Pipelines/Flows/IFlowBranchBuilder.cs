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
        /// Assigns a display name to the branch.
        /// </summary>
        /// <param name="name">The display name to show in generated documentation.</param>
        /// <returns>The current branch builder.</returns>
        IFlowBranchBuilder<TCurrent, TNext> Named(string name);

        /// <summary>
        /// Assigns a short description to the branch.
        /// </summary>
        /// <param name="description">The description to show in generated documentation.</param>
        /// <returns>The current branch builder.</returns>
        IFlowBranchBuilder<TCurrent, TNext> Describe(string description);

        /// <summary>
        /// Adds descriptive tags to the branch.
        /// </summary>
        /// <param name="tags">The tags to show in generated documentation.</param>
        /// <returns>The current branch builder.</returns>
        IFlowBranchBuilder<TCurrent, TNext> Tags(params string[] tags);

        /// <summary>
        /// Adds custom key-value metadata to the branch.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The current branch builder.</returns>
        IFlowBranchBuilder<TCurrent, TNext> Metadata(string key, string value);

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
