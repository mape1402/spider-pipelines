namespace Spider.Pipelines.Samples.Web
{
    using Spider.Pipelines.Core;

    /// <summary>
    /// Handles credit applications in the sample pipeline.
    /// </summary>
    public sealed class CreditApplicationHandler
    {
        private readonly ISpider _spider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditApplicationHandler"/> class.
        /// </summary>
        /// <param name="spider">The Spider facade used to compose handler-level flows.</param>
        public CreditApplicationHandler(ISpider spider)
        {
            _spider = spider ?? throw new ArgumentNullException(nameof(spider));
        }

        /// <summary>
        /// Handles the credit application request.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The credit decision.</returns>
        public Task<CreditDecision> HandleAsync(
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
            => _spider
                .ComposeFlow<CreditApplicationRequest, CreditDecision>("Handle credit application")
                .Then(CreditEvaluationFlowSteps.ValidateApplicationAsync)
                .Then(CreditEvaluationFlowSteps.BuildBureauRequest)
                .Branch<BureauDecision>(branch => branch
                    .When(CreditEvaluationFlowSteps.CanUseCachedBureau, cached => cached.Then(CreditEvaluationFlowSteps.UseCachedBureau))
                    .Otherwise(remote => remote
                        .Then(CreditEvaluationFlowSteps.CallBureauAsync)
                        .Then(CreditEvaluationFlowSteps.EvaluateBureauResponse)))
                .ThenWith<CreditApplicationRequest, BureauDecision, RiskInput>(CreditEvaluationFlowSteps.BuildRiskInput)
                .Then(CreditEvaluationFlowSteps.CalculateRiskAsync)
                .ThenWith<CreditApplicationRequest, RiskScore, CreditDecision>(CreditEvaluationFlowSteps.BuildDecision)
                .RunAsync(request, cancellationToken);
    }
}
