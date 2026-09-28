namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents a dispatch plan after the selected dispatch lane has been resolved.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Channel">The resolved dispatch channel.</param>
    /// <param name="Owner">The owner responsible for the resolved dispatch lane.</param>
    public sealed record ResolvedOfferDispatchPlan(
        string ApplicationId,
        string Channel,
        string Owner);
}
