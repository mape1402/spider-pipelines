namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Stores configured Spider flow profiles.
    /// </summary>
    public sealed class FlowProfileRegistry
    {
        private readonly Dictionary<string, FlowProfileOptions> _profiles = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds or replaces a profile.
        /// </summary>
        /// <param name="name">The profile name.</param>
        /// <param name="options">The profile options.</param>
        public void Set(string name, FlowProfileOptions options)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Profile name is required.", nameof(name));

            _profiles[name] = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Attempts to get a profile by name.
        /// </summary>
        /// <param name="name">The profile name.</param>
        /// <param name="options">The profile options when the profile exists.</param>
        /// <returns><see langword="true"/> when the profile exists; otherwise, <see langword="false"/>.</returns>
        public bool TryGet(string name, out FlowProfileOptions options)
            => _profiles.TryGetValue(name, out options);
    }
}
