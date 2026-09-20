namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents a bureau status request.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="CustomerId">The customer identifier.</param>
    /// <param name="Amount">The requested amount.</param>
    public sealed record BureauRequest(
        string ApplicationId,
        string CustomerId,
        decimal Amount);
}
