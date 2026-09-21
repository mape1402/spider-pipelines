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
                .Describe("Evaluates bureau and risk signals before producing the final credit decision.")
                .Tags("credit", "bureau", "risk")
                .Metadata("audience", "credit operations")
                .UsingProfile("Web verbose telemetry")
                .Then(CreditEvaluationFlowSteps.ValidateApplicationAsync, step => step
                    .Named("Validate application")
                    .Describe("Checks that the sample credit application can continue through the evaluation flow.")
                    .Tags("validation", "guard"))
                .Then(CreditEvaluationFlowSteps.BuildBureauRequest, step => step
                    .Named("Build bureau request")
                    .Describe("Maps the incoming credit application into the bureau request contract.")
                    .Tags("mapping", "bureau"))
                .Branch<BureauDecision>(branch => branch
                    .Named("Bureau decision")
                    .Describe("Chooses whether to reuse cached bureau data or call the remote bureau service.")
                    .Tags("branch", "bureau")
                    .When(CreditEvaluationFlowSteps.CanUseCachedBureau, cached => cached
                        .Named("Use cached bureau")
                        .Describe("Uses already available bureau information when the request qualifies.")
                        .Tags("cache")
                        .Then(CreditEvaluationFlowSteps.UseCachedBureau, step => step
                            .Named("Use cached bureau decision")
                            .Tags("cache", "bureau")))
                    .Otherwise(remote => remote
                        .Named("Call bureau")
                        .Describe("Calls the external bureau path when cached data cannot be used.")
                        .Tags("remote", "bureau")
                        .Then(CreditEvaluationFlowSteps.CallBureauAsync, step => step
                            .Named("Call bureau service")
                            .Tags("remote-call"))
                        .Then(CreditEvaluationFlowSteps.EvaluateBureauResponse, step => step
                            .Named("Evaluate bureau response")
                            .Tags("evaluation"))))
                .ThenWith<CreditApplicationRequest, BureauDecision, RiskInput>(CreditEvaluationFlowSteps.BuildRiskInput, step => step
                    .Named("Build risk input")
                    .Tags("mapping", "risk"))
                .Then(CreditEvaluationFlowSteps.CalculateRiskAsync, step => step
                    .Named("Calculate risk")
                    .Describe("Calculates the score used to approve, reject, or review the application.")
                    .Tags("risk", "scoring"))
                .ThenWith<CreditApplicationRequest, RiskScore, CreditDecision>(CreditEvaluationFlowSteps.BuildDecision, step => step
                    .Named("Build credit decision")
                    .Tags("decision"))
                .Then(workflow.PersistDecisionAsync, step => step
                    .Named("Persist decision")
                    .Describe("Delegates persistence to the nested decision flow.")
                    .Tags("persistence", "linked-flow"))
                .RunAsync(request, cancellationToken);

            _ = spider
                .ComposeFlow<CreditDecision>("Publish credit decision")
                .Describe("Publishes a notification when a credit decision is ready to leave the service.")
                .Tags("notification", "background")
                .UsingProfile("Background telemetry")
                .ContinueIf(CreditDecisionPublishingSteps.ShouldPublishDecision, Flow.Stop(), step => step
                    .Named("Should publish decision")
                    .Describe("Stops the flow when the decision is not ready to be published.")
                    .Tags("guard"))
                .Then(CreditDecisionPublishingSteps.BuildNotificationEnvelope, step => step
                    .Named("Build notification envelope")
                    .Tags("mapping", "notification"))
                .Then(CreditDecisionPublishingSteps.PublishNotificationAsync, step => step
                    .Named("Publish notification")
                    .Tags("publish", "notification"))
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
