namespace Spider.Pipelines.Web
{
    using Spider.Pipelines.Architecture;

    /// <summary>
    /// Serializes Spider architecture manifests for web visualization.
    /// </summary>
    public interface ISpiderArchitectureManifestSerializer
    {
        /// <summary>
        /// Serializes the specified manifest.
        /// </summary>
        /// <param name="manifest">The manifest to serialize.</param>
        /// <returns>The serialized manifest.</returns>
        string Serialize(SpiderArchitectureManifest manifest);
    }
}
