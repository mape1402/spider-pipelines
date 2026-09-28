namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the plan used to dispatch a generated credit offer.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Channel">The dispatch channel.</param>
    /// <param name="HighPriority">A value indicating whether the offer requires priority dispatch.</param>
    /// <param name="Owner">The owner responsible for dispatching the offer.</param>
    public sealed record OfferDispatchPlan(
        string ApplicationId,
        string Channel,
        bool HighPriority,
        string Owner);
}
