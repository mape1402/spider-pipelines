namespace Spider.Pipelines.Architecture
{
    /// <summary>
    /// Describes an architecture component discovered by Spider.
    /// </summary>
    public sealed class SpiderComponentDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderComponentDescriptor"/> class.
        /// </summary>
        /// <param name="id">The stable component id.</param>
        /// <param name="kind">The component kind.</param>
        /// <param name="displayName">The component display name.</param>
        /// <param name="metadata">The component metadata.</param>
        public SpiderComponentDescriptor(
            string id,
            string kind,
            string displayName,
            IReadOnlyDictionary<string, string> metadata)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Component id is required.", nameof(id)) : id;
            Kind = string.IsNullOrWhiteSpace(kind) ? throw new ArgumentException("Component kind is required.", nameof(kind)) : kind;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
            Metadata = metadata ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the stable component id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the component kind.
        /// </summary>
        public string Kind { get; }

        /// <summary>
        /// Gets the component display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the component metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
