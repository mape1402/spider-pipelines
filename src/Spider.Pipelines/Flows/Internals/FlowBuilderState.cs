namespace Spider.Pipelines.Flows.Internals
{
    /// <summary>
    /// Stores immutable builder state shared across typed flow builders.
    /// </summary>
    internal sealed class FlowBuilderState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowBuilderState"/> class.
        /// </summary>
        /// <param name="name">The logical flow name.</param>
        public FlowBuilderState(string name)
            : this(name, null, new Dictionary<string, string>(), Array.Empty<IFlowStep>())
        {
        }

        private FlowBuilderState(
            string name,
            string profileName,
            IReadOnlyDictionary<string, string> metadata,
            IReadOnlyList<IFlowStep> steps)
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Flow name is required.", nameof(name))
                : name;
            ProfileName = profileName;
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        /// <summary>
        /// Gets the logical flow name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the selected profile name.
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// Gets the configured descriptive metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// Gets the configured flow steps.
        /// </summary>
        public IReadOnlyList<IFlowStep> Steps { get; }

        /// <summary>
        /// Creates new state with the selected profile.
        /// </summary>
        /// <param name="profileName">The selected profile name.</param>
        /// <returns>The updated state.</returns>
        public FlowBuilderState WithProfile(string profileName)
            => new(Name, profileName, Metadata, Steps);

        /// <summary>
        /// Creates new state with additional descriptive metadata.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The updated state.</returns>
        public FlowBuilderState WithMetadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Metadata key is required.", nameof(key));

            var metadata = new Dictionary<string, string>(Metadata, StringComparer.Ordinal);
            if (!string.IsNullOrWhiteSpace(value))
                metadata[key] = value;

            return new(Name, ProfileName, metadata, Steps);
        }

        /// <summary>
        /// Creates new state with an appended step.
        /// </summary>
        /// <param name="step">The step to append.</param>
        /// <returns>The updated state.</returns>
        public FlowBuilderState AddStep(IFlowStep step)
            => new(Name, ProfileName, Metadata, Steps.Concat(new[] { step }).ToArray());
    }
}
