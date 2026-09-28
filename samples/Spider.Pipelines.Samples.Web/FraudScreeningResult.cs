namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents the result of fraud screening for a credit application.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="IsCleared">A value indicating whether fraud screening cleared the request.</param>
    /// <param name="ReviewLevel">The review level used to screen the request.</param>
    /// <param name="Reason">The business reason produced by the screening path.</param>
    public sealed record FraudScreeningResult(
        string ApplicationId,
        bool IsCleared,
        string ReviewLevel,
        string Reason);
}
