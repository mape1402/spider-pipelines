namespace Spider.Pipelines.Architecture
{
    /// <summary>
    /// Provides Spider architecture metadata for external graph consumers.
    /// </summary>
    public interface ISpiderArchitectureProvider
    {
        /// <summary>
        /// Gets the current Spider architecture manifest.
        /// </summary>
        /// <returns>The current Spider architecture manifest.</returns>
        SpiderArchitectureManifest GetManifest();
    }
}
