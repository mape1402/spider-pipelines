namespace Spider.Pipelines.Flows.Internals
{
    using Spider.Pipelines.Flows;

    /// <summary>
    /// Captures descriptive flow metadata without affecting runtime execution.
    /// </summary>
    internal sealed class FlowMetadataBuilder : IFlowMetadataBuilder
    {
        private readonly Dictionary<string, string> _metadata = new(StringComparer.Ordinal);

        /// <inheritdoc/>
        public IFlowMetadataBuilder Named(string name)
        {
            Set("name", name);
            return this;
        }

        /// <inheritdoc/>
        public IFlowMetadataBuilder Describe(string description)
        {
            Set("description", description);
            return this;
        }

        /// <inheritdoc/>
        public IFlowMetadataBuilder Tags(params string[] tags)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            Set("tags", string.Join(",", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim())));
            return this;
        }

        /// <inheritdoc/>
        public IFlowMetadataBuilder Metadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Metadata key is required.", nameof(key));

            Set(key, value);
            return this;
        }

        /// <summary>
        /// Gets captured metadata for future runtime integrations.
        /// </summary>
        public IReadOnlyDictionary<string, string> MetadataValues => _metadata;

        private void Set(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                _metadata[key] = value;
        }
    }
}
