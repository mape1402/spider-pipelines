namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the input for risk evaluation.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Amount">The requested amount.</param>
    /// <param name="BureauApproved">A value indicating whether bureau approved the customer.</param>
    public sealed record RiskInput(
        string ApplicationId,
        decimal Amount,
        bool BureauApproved);
}
