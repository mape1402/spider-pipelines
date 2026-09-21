namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Contains the executable steps used by the credit evaluation flow sample.
    /// </summary>
    public static class CreditEvaluationFlowSteps
    {
        /// <summary>
        /// Validates the application before external checks are executed.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous validation.</returns>
        public static Task ValidateApplicationAsync(
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
        public static BureauRequest BuildBureauRequest(CreditApplicationRequest request)
            => new(request.ApplicationId, request.CustomerId, request.RequestedAmount);

        /// <summary>
        /// Determines whether cached bureau data can be used.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <returns><see langword="true"/> when cached data is enough; otherwise, <see langword="false"/>.</returns>
        public static bool CanUseCachedBureau(BureauRequest request)
            => request.Amount <= 1000m;

        /// <summary>
        /// Produces a bureau decision from cached data.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <returns>The cached bureau decision.</returns>
        public static BureauDecision UseCachedBureau(BureauRequest request)
            => new(request.ApplicationId, "Cached", true);

        /// <summary>
        /// Calls the remote bureau service.
        /// </summary>
        /// <param name="request">The bureau request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The remote bureau response.</returns>
        public static Task<BureauResponse> CallBureauAsync(
            BureauRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(new BureauResponse(request.ApplicationId, 720, true));

        /// <summary>
        /// Evaluates the remote bureau response.
        /// </summary>
        /// <param name="response">The remote bureau response.</param>
        /// <returns>The bureau decision.</returns>
        public static BureauDecision EvaluateBureauResponse(BureauResponse response)
            => new(response.ApplicationId, "Remote", response.Score >= 650 && response.IsActive);

        /// <summary>
        /// Builds the risk input from the application and bureau decision.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="bureau">The bureau decision.</param>
        /// <returns>The risk input.</returns>
        public static RiskInput BuildRiskInput(
            CreditApplicationRequest request,
            BureauDecision bureau)
            => new(request.ApplicationId, request.RequestedAmount, bureau.IsApproved);

        /// <summary>
        /// Calculates a risk score for the application.
        /// </summary>
        /// <param name="input">The risk input.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The calculated risk score.</returns>
        public static Task<RiskScore> CalculateRiskAsync(
            RiskInput input,
            CancellationToken cancellationToken)
            => Task.FromResult(new RiskScore(input.ApplicationId, input.BureauApproved ? 82 : 35));

        /// <summary>
        /// Builds the final credit decision.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="score">The risk score.</param>
        /// <returns>The final credit decision.</returns>
        public static CreditDecision BuildDecision(
            CreditApplicationRequest request,
            RiskScore score)
            => new(request.ApplicationId, score.Value >= 70 ? "Approved" : "Rejected", score.Value);
    }
}
