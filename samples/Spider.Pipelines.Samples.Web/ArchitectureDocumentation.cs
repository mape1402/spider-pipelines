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
                    .Named("Credit decision pipeline")
                    .Describe("Wraps the credit application handler with request preparation, telemetry, and post-processing hooks.")
                    .Purpose("Protects and observes credit decision execution without hiding the underlying business flow.")
                    .Trigger("POST /_spider/sample/{scenario}")
                    .Wraps("CreditApplicationHandler.HandleAsync")
                    .Input(nameof(CreditApplicationRequest))
                    .Output(nameof(CreditDecision))
                    .Policies("telemetry", "audit", "fault-capture")
                    .FailureBehavior("Failure hooks run and runtime traces are flushed before the endpoint responds.")
                    .Module("Credit evaluation")
                    .PreProcess((ctx, args) => Task.CompletedTask, stage => stage
                        .Named("Prepare request context")
                        .Describe("Initializes request-scoped context before the handler executes.")
                        .Purpose("Creates the correlation and request envelope used by downstream telemetry.")
                        .Policies("correlation", "request-normalization")
                        .Observability("request-context-created", "trace-enrichment")
                        .Timeout("Runs before handler dispatch; expected to stay below 10 ms.")
                        .Tags("context", "pre-flight"))
                    .UseMiddleware((ctx, next) => next(), stage => stage
                        .Named("Trace handler execution")
                        .Describe("Wraps the target handler so runtime tracing can see the business operation.")
                        .Purpose("Captures the target handler as a runtime span without changing handler behavior.")
                        .Wraps("CreditApplicationHandler.HandleAsync")
                        .Policies("runtime-tracing", "transparent-wrapper")
                        .Observability("handler-started", "handler-completed", "handler-faulted")
                        .Tags("telemetry", "middleware"))
                    .Parallel((ctx, args) => Task.CompletedTask, stage => stage
                        .Named("Collect async side signals")
                        .Describe("Runs non-blocking side work that does not change the credit decision result.")
                        .Purpose("Documents fire-and-forget enrichment work that can run beside the main operation.")
                        .Policies("non-blocking", "best-effort")
                        .Observability("side-signal-collected")
                        .FailureBehavior("Failures are recorded as trace metadata and do not replace the handler result.")
                        .Tags("parallel", "signals"))
                    .OnSuccess((ctx, args) => Task.CompletedTask, stage => stage
                        .Named("Publish success outcome")
                        .Describe("Records the successful credit decision path.")
                        .Purpose("Publishes audit information after a successful credit decision.")
                        .Policies("audit", "success-only")
                        .Observability("credit-decision-success")
                        .RelatedFlow<CreditDecision, CreditDecision>()
                        .Tags("success", "audit"))
                    .OnFailure((ctx, args) => Task.CompletedTask, stage => stage
                        .Named("Capture failure outcome")
                        .Describe("Records failure information without replacing the original exception.")
                        .Purpose("Preserves diagnostic context for failed credit decisions.")
                        .Policies("fault-capture", "rethrow-original")
                        .Observability("credit-decision-fault")
                        .FailureBehavior("Records failure metadata and lets the original exception continue.")
                        .Tags("failure", "audit")))
                .ExecuteAsync(service => (application, token) => service.HandleAsync(application, token), request, cancellationToken);
        }
    }
}
