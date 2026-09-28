namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the information required to price a credit offer.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="RequestedAmount">The requested credit amount.</param>
    /// <param name="FraudCleared">A value indicating whether fraud screening cleared the request.</param>
    /// <param name="ReviewLevel">The review level assigned during screening.</param>
    /// <param name="ExposureTier">The exposure tier used to choose pricing.</param>
    public sealed record PricingInput(
        string ApplicationId,
        decimal RequestedAmount,
        bool FraudCleared,
        string ReviewLevel,
        int ExposureTier);
}
