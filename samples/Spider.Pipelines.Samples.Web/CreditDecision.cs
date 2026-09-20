namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the final credit decision.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Status">The decision status.</param>
    /// <param name="Score">The calculated risk score.</param>
    public sealed record CreditDecision(
        string ApplicationId,
        string Status,
        int Score);
}
