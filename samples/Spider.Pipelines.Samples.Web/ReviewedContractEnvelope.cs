namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents a contract envelope after the selected review lane has completed.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Template">The contract template.</param>
    /// <param name="ReviewLane">The review lane that produced the contract terms.</param>
    /// <param name="Terms">The reviewed contract terms.</param>
    public sealed record ReviewedContractEnvelope(
        string ApplicationId,
        string Template,
        string ReviewLane,
        string Terms);
}
