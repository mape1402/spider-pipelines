namespace Spider.Pipelines.Samples.Web
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.Flows;

    /// <summary>
    /// Declares sample flows and pipelines for compile-time architecture documentation.
    /// </summary>
    public sealed class ArchitectureDocumentation
    {
        /// <summary>
        /// Describes the credit evaluation process that the source generator converts into metadata.
        /// </summary>
        /// <param name="spider">The Spider facade used to compose flows and pipelines.</param>
        /// <param name="workflow">The workflow that exposes nested decision flows.</param>
        /// <param name="request">The request that starts the sample flow.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public void Describe(
            ISpider spider,
            CreditDecisionWorkflow workflow,
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
        {
            _ = spider
                .ComposeFlow<CreditApplicationRequest, CreditDecision>("Evaluate credit application")
                .UsingProfile("Web verbose telemetry")
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
                .Then(workflow.PersistDecisionAsync)
                .RunAsync(request, cancellationToken);

            _ = spider
                .ComposeFlow<CreditDecision>("Publish credit decision")
                .UsingProfile("Background telemetry")
                .ContinueIf(CreditDecisionPublishingSteps.ShouldPublishDecision, Flow.Stop())
                .Then(CreditDecisionPublishingSteps.BuildNotificationEnvelope)
                .Then(CreditDecisionPublishingSteps.PublishNotificationAsync)
                .RunAsync(new CreditDecision(request.ApplicationId, "Pending", 0), cancellationToken);

            _ = spider
                .InitBridge<CreditApplicationHandler>()
                .Attach<CreditApplicationRequest, CreditDecision>(builder => builder
                    .PreProcess((ctx, args) => Task.CompletedTask)
                    .UseMiddleware((ctx, next) => next())
                    .Parallel((ctx, args) => Task.CompletedTask)
                    .OnSuccess((ctx, args) => Task.CompletedTask)
                    .OnFailure((ctx, args) => Task.CompletedTask))
                .ExecuteAsync(service => (application, token) => service.HandleAsync(application, token), request, cancellationToken);
        }
    }
}
