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
                .Describe("Handles a credit application inside the endpoint pipeline.")
                .Tags("handler", "credit")
                .Then(CreditEvaluationFlowSteps.ValidateApplicationAsync, step => step
                    .Named("Validate application")
                    .Tags("validation"))
                .Then(CreditEvaluationFlowSteps.BuildBureauRequest, step => step
                    .Named("Build bureau request")
                    .Tags("bureau", "mapping"))
                .Branch<BureauDecision>(branch => branch
                    .Named("Bureau decision")
                    .Tags("branch", "bureau")
                    .When(CreditEvaluationFlowSteps.CanUseCachedBureau, cached => cached
                        .Named("Use cached bureau")
                        .Then(CreditEvaluationFlowSteps.UseCachedBureau, step => step.Named("Use cached bureau decision")))
                    .Otherwise(remote => remote
                        .Named("Call bureau")
                        .Then(CreditEvaluationFlowSteps.CallBureauAsync, step => step.Named("Call bureau service"))
                        .Then(CreditEvaluationFlowSteps.EvaluateBureauResponse, step => step.Named("Evaluate bureau response"))))
                .ThenWith<CreditApplicationRequest, BureauDecision, RiskInput>(CreditEvaluationFlowSteps.BuildRiskInput, step => step
                    .Named("Build risk input"))
                .Then(CreditEvaluationFlowSteps.CalculateRiskAsync, step => step
                    .Named("Calculate risk"))
                .ThenWith<CreditApplicationRequest, RiskScore, CreditDecision>(CreditEvaluationFlowSteps.BuildDecision, step => step
                    .Named("Build credit decision"))
                .RunAsync(request, cancellationToken);
    }
}
