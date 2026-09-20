namespace Spider.Pipelines.Web
{
    using Spider.Pipelines.Architecture;

    /// <summary>
    /// Renders Spider architecture manifests as graphical web documentation.
    /// </summary>
    public interface ISpiderArchitectureWebRenderer
    {
        /// <summary>
        /// Renders the specified manifest.
        /// </summary>
        /// <param name="manifest">The manifest to render.</param>
        /// <param name="options">The rendering options.</param>
        /// <returns>A complete HTML document.</returns>
        string Render(SpiderArchitectureManifest manifest, SpiderArchitectureWebOptions options = null);
    }
}
