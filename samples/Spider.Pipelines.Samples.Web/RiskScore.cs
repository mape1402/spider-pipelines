namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents a calculated risk score.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Value">The score value.</param>
    public sealed record RiskScore(
        string ApplicationId,
        int Value);
}
