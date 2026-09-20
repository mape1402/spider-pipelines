namespace Spider.Pipelines.Samples.Web
{
    /// <summary>
    /// Represents a remote bureau response.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <param name="Score">The bureau score.</param>
    /// <param name="IsActive">A value indicating whether the bureau file is active.</param>
    public sealed record BureauResponse(
        string ApplicationId,
        int Score,
        bool IsActive);
}
