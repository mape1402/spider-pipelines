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
        /// <param name="request">The request that starts the sample flow.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public void Describe(
            ISpider spider,
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
        {
            _ = spider
                .ComposeFlow<CreditApplicationRequest, CreditDecision>("Evaluate credit application")
                .UsingProfile("Web verbose telemetry")
                .Then(ValidateApplicationAsync)
                .Then(BuildBureauRequest)
                .Branch<BureauDecision>(branch => branch
                    .When(CanUseCachedBureau, cached => cached.Then(UseCachedBureau))
                    .Otherwise(remote => remote
                        .Then(CallBureauAsync)
                        .Then(EvaluateBureauResponse)))
                .ThenWith<CreditApplicationRequest, BureauDecision, RiskInput>(BuildRiskInput)
                .Then(CalculateRiskAsync)
                .ThenWith<CreditApplicationRequest, RiskScore, CreditDecision>(BuildDecision)
                .RunAsync(request, cancellationToken);

            _ = spider
                .ComposeFlow<CreditDecision>("Publish credit decision")
                .UsingProfile("Background telemetry")
                .ContinueIf(ShouldPublishDecision, Flow.Stop())
                .Then(BuildNotificationEnvelope)
                .Then(PublishNotificationAsync)
                .RunAsync(new CreditDecision(request.ApplicationId, "Pending", 0), cancellationToken);

            spider
                .InitBridge<CreditApplicationHandler>()
                .Attach<CreditApplicationRequest, CreditDecision>(builder => builder
                    .PreProcess((ctx, args) => Task.CompletedTask)
                    .UseMiddleware((ctx, next) => next())
                    .Parallel((ctx, args) => Task.CompletedTask)
                    .OnSuccess((ctx, args) => Task.CompletedTask)
                    .OnFailure((ctx, args) => Task.CompletedTask));
        }

        /// <summary>
        /// Validates the application before external checks are executed.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous validation.</returns>
        private static Task ValidateApplicationAsync(
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerId))
                throw new InvalidOperationException("Customer id is required.");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds the bureau request from the credit application.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <returns>The request sent to bureau services.</returns>
        private static BureauRequest BuildBureauRequest(CreditApplicationRequest request)
            => new(request.ApplicationId, request.CustomerId, request.RequestedAmount);

        /// <summary>
        /// Determines whether cached bureau data can be used.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <returns><see langword="true"/> when cached data is enough; otherwise, <see langword="false"/>.</returns>
        private static bool CanUseCachedBureau(BureauRequest request)
            => request.Amount <= 1000m;

        /// <summary>
        /// Produces a bureau decision from cached data.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <returns>The cached bureau decision.</returns>
        private static BureauDecision UseCachedBureau(BureauRequest request)
            => new(request.ApplicationId, "Cached", true);

        /// <summary>
        /// Calls the remote bureau service.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The remote bureau response.</returns>
        private static Task<BureauResponse> CallBureauAsync(
            BureauRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(new BureauResponse(request.ApplicationId, 720, true));

        /// <summary>
        /// Evaluates the remote bureau response.
        /// </summary>
        /// <param name="response">The remote bureau response.</param>
        /// <returns>The bureau decision.</returns>
        private static BureauDecision EvaluateBureauResponse(BureauResponse response)
            => new(response.ApplicationId, "Remote", response.Score >= 650 && response.IsActive);

        /// <summary>
        /// Builds the risk input from the application and bureau decision.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="bureau">The bureau decision.</param>
        /// <returns>The risk input.</returns>
        private static RiskInput BuildRiskInput(
            CreditApplicationRequest request,
            BureauDecision bureau)
            => new(request.ApplicationId, request.RequestedAmount, bureau.IsApproved);

        /// <summary>
        /// Calculates a risk score for the application.
        /// </summary>
        /// <param name="input">The risk input.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The calculated risk score.</returns>
        private static Task<RiskScore> CalculateRiskAsync(
            RiskInput input,
            CancellationToken cancellationToken)
            => Task.FromResult(new RiskScore(input.ApplicationId, input.BureauApproved ? 82 : 35));

        /// <summary>
        /// Builds the final credit decision.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="score">The risk score.</param>
        /// <returns>The final credit decision.</returns>
        private static CreditDecision BuildDecision(
            CreditApplicationRequest request,
            RiskScore score)
            => new(request.ApplicationId, score.Value >= 70 ? "Approved" : "Rejected", score.Value);

        /// <summary>
        /// Determines whether the decision should be published.
        /// </summary>
        /// <param name="decision">The credit decision.</param>
        /// <returns><see langword="true"/> when the decision should be published; otherwise, <see langword="false"/>.</returns>
        private static bool ShouldPublishDecision(CreditDecision decision)
            => decision.Status != "Pending";

        /// <summary>
        /// Builds a notification envelope from a credit decision.
        /// </summary>
        /// <param name="decision">The credit decision.</param>
        /// <returns>The notification envelope.</returns>
        private static NotificationEnvelope BuildNotificationEnvelope(CreditDecision decision)
            => new(decision.ApplicationId, $"Decision: {decision.Status}");

        /// <summary>
        /// Publishes the notification envelope.
        /// </summary>
        /// <param name="envelope">The notification envelope.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous publish operation.</returns>
        private static Task PublishNotificationAsync(
            NotificationEnvelope envelope,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
