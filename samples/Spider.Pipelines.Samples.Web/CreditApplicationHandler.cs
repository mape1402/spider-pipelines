namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Handles credit applications in the sample pipeline.
    /// </summary>
    public sealed class CreditApplicationHandler
    {
        /// <summary>
        /// Handles the credit application request.
        /// </summary>
        /// <param name="request">The credit application request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The credit decision.</returns>
        public Task<CreditDecision> HandleAsync(
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(new CreditDecision(request.ApplicationId, "Approved", 82));
    }
}
