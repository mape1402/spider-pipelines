namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the bureau evaluation decision.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Source">The source used for the decision.</param>
    /// <param name="IsApproved">A value indicating whether the bureau evaluation passed.</param>
    public sealed record BureauDecision(
        string ApplicationId,
        string Source,
        bool IsApproved);
}
