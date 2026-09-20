namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents a notification envelope produced by a flow.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Message">The notification message.</param>
    public sealed record NotificationEnvelope(
        string ApplicationId,
        string Message);
}
