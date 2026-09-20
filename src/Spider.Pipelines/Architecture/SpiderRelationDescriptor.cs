namespace Spider.Pipelines.Architecture
{
    /// <summary>
    /// Describes an architecture relation discovered by Spider.
    /// </summary>
    public sealed class SpiderRelationDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderRelationDescriptor"/> class.
        /// </summary>
        /// <param name="id">The stable relation id.</param>
        /// <param name="sourceId">The source component id.</param>
        /// <param name="targetId">The target component id.</param>
        /// <param name="kind">The relation kind.</param>
        /// <param name="metadata">The relation metadata.</param>
        public SpiderRelationDescriptor(
            string id,
            string sourceId,
            string targetId,
            string kind,
            IReadOnlyDictionary<string, string> metadata)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Relation id is required.", nameof(id)) : id;
            SourceId = string.IsNullOrWhiteSpace(sourceId) ? throw new ArgumentException("Source id is required.", nameof(sourceId)) : sourceId;
            TargetId = string.IsNullOrWhiteSpace(targetId) ? throw new ArgumentException("Target id is required.", nameof(targetId)) : targetId;
            Kind = string.IsNullOrWhiteSpace(kind) ? throw new ArgumentException("Relation kind is required.", nameof(kind)) : kind;
            Metadata = metadata ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the stable relation id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the source component id.
        /// </summary>
        public string SourceId { get; }

        /// <summary>
        /// Gets the target component id.
        /// </summary>
        public string TargetId { get; }

        /// <summary>
        /// Gets the relation kind.
        /// </summary>
        public string Kind { get; }

        /// <summary>
        /// Gets the relation metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
