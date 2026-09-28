namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the normalized information used by underwriting decisions.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="CustomerId">The customer identifier.</param>
    /// <param name="RequestedAmount">The requested credit amount.</param>
    /// <param name="BureauSource">The bureau source used to qualify the request.</param>
    /// <param name="BureauApproved">A value indicating whether the bureau decision approved the request.</param>
    /// <param name="ExposureTier">The exposure tier calculated for the request.</param>
    public sealed record UnderwritingPacket(
        string ApplicationId,
        string CustomerId,
        decimal RequestedAmount,
        string BureauSource,
        bool BureauApproved,
        int ExposureTier);
}
