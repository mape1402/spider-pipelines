namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents a credit application request.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="CustomerId">The customer identifier.</param>
    /// <param name="RequestedAmount">The requested credit amount.</param>
    public sealed record CreditApplicationRequest(
        string ApplicationId,
        string CustomerId,
        decimal RequestedAmount);
}
