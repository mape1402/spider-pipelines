namespace Spider.Pipelines.Targeting
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Represents a delegate used to determine whether an override method should be executed 
    /// instead of the default pipeline operation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being processed.</typeparam>
    /// <param name="context">
    /// The read-only pipeline context, including request data and execution state.
    /// </param>
    /// <param name="arguments">
    /// Additional arguments provided for the condition evaluation.
    /// </param>
    /// <returns>
    /// <c>true</c> to indicate that the override should be executed; otherwise, <c>false</c>.
    /// </returns>
    public delegate bool OverridesConditionDelegate<TRequest>(
        IReadOnlyContext<TRequest> context,
        OverridesConditionArguments arguments);
}
