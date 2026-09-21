namespace Spider.Pipelines.Samples.Web
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Coordinates decision-level flows used by the sample documentation.
    /// </summary>
    public sealed class CreditDecisionWorkflow
    {
        private readonly ISpider _spider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditDecisionWorkflow"/> class.
        /// </summary>
        /// <param name="spider">The Spider facade used to compose nested flows.</param>
        public CreditDecisionWorkflow(ISpider spider)
        {
            _spider = spider ?? throw new ArgumentNullException(nameof(spider));
        }

        /// <summary>
        /// Persists a credit decision through a nested flow.
        /// </summary>
        /// <param name="decision">The decision to persist.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The persisted decision.</returns>
        public Task<CreditDecision> PersistDecisionAsync(
            CreditDecision decision,
            CancellationToken cancellationToken)
            => _spider
                .ComposeFlow<CreditDecision, CreditDecision>("Persist credit decision")
                .Describe("Persists the credit decision and returns the stored model.")
                .Tags("persistence", "nested-flow")
                .Then(CreditDecisionPersistenceSteps.StoreDecisionAsync, step => step
                    .Named("Store decision")
                    .Tags("storage"))
                .Then(CreditDecisionPersistenceSteps.MarkDecisionAsStored, step => step
                    .Named("Mark decision as stored")
                    .Tags("state"))
                .RunAsync(decision, cancellationToken);
    }
}
