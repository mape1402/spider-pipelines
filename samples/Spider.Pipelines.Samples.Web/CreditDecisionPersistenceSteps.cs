namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Contains the executable steps used by the credit decision persistence flow sample.
    /// </summary>
    public static class CreditDecisionPersistenceSteps
    {
        /// <summary>
        /// Stores the credit decision.
        /// </summary>
        /// <param name="decision">The decision to store.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The stored decision.</returns>
        public static Task<CreditDecision> StoreDecisionAsync(
            CreditDecision decision,
            CancellationToken cancellationToken)
            => Task.FromResult(decision);

        /// <summary>
        /// Marks the decision as stored for documentation purposes.
        /// </summary>
        /// <param name="decision">The stored decision.</param>
        /// <returns>The updated decision.</returns>
        public static CreditDecision MarkDecisionAsStored(CreditDecision decision)
            => new(decision.ApplicationId, decision.Status, decision.Score);
    }
}
