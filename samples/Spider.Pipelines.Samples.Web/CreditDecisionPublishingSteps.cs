namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Contains the executable steps used by the credit decision publishing flow sample.
    /// </summary>
    public static class CreditDecisionPublishingSteps
    {
        /// <summary>
        /// Determines whether the decision should be published.
        /// </summary>
        /// <param name="decision">The credit decision.</param>
        /// <returns><see langword="true"/> when the decision should be published; otherwise, <see langword="false"/>.</returns>
        public static bool ShouldPublishDecision(CreditDecision decision)
            => decision.Status != "Pending";

        /// <summary>
        /// Builds a notification envelope from a credit decision.
        /// </summary>
        /// <param name="decision">The credit decision.</param>
        /// <returns>The notification envelope.</returns>
        public static NotificationEnvelope BuildNotificationEnvelope(CreditDecision decision)
            => new(decision.ApplicationId, $"Decision: {decision.Status}");

        /// <summary>
        /// Publishes the notification envelope.
        /// </summary>
        /// <param name="envelope">The notification envelope.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous publish operation.</returns>
        public static Task PublishNotificationAsync(
            NotificationEnvelope envelope,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
