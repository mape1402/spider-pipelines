namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the contract information prepared for an approved offer.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Template">The contract template.</param>
    /// <param name="RequiresLegalReview">A value indicating whether legal review is required.</param>
    /// <param name="Terms">The terms attached to the contract.</param>
    public sealed record ContractEnvelope(
        string ApplicationId,
        string Template,
        bool RequiresLegalReview,
        string Terms);
}
