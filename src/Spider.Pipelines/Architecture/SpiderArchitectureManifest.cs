namespace Spider.Pipelines.Architecture
{
    /// <summary>
    /// Represents architecture metadata discovered by Spider.
    /// </summary>
    public sealed class SpiderArchitectureManifest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderArchitectureManifest"/> class.
        /// </summary>
        /// <param name="components">The discovered components.</param>
        /// <param name="relations">The discovered relations.</param>
        public SpiderArchitectureManifest(
            IReadOnlyCollection<SpiderComponentDescriptor> components,
            IReadOnlyCollection<SpiderRelationDescriptor> relations)
        {
            Components = components ?? throw new ArgumentNullException(nameof(components));
            Relations = relations ?? throw new ArgumentNullException(nameof(relations));
        }

        /// <summary>
        /// Gets the producer identifier.
        /// </summary>
        public string ProducerId => "Spider.Pipelines";

        /// <summary>
        /// Gets the manifest schema version.
        /// </summary>
        public string ManifestVersion => "1.0";

        /// <summary>
        /// Gets the discovered components.
        /// </summary>
        public IReadOnlyCollection<SpiderComponentDescriptor> Components { get; }

        /// <summary>
        /// Gets the discovered relations.
        /// </summary>
        public IReadOnlyCollection<SpiderRelationDescriptor> Relations { get; }
    }
}
