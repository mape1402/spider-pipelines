namespace Spider.Pipelines.Flows
{
    /// <summary>
    /// Represents a configured flow profile registration.
    /// </summary>
    public sealed class FlowProfileRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowProfileRegistration"/> class.
        /// </summary>
        /// <param name="name">The profile name.</param>
        /// <param name="options">The profile options.</param>
        public FlowProfileRegistration(string name, FlowProfileOptions options)
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Profile name is required.", nameof(name))
                : name;
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the profile name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the profile options.
        /// </summary>
        public FlowProfileOptions Options { get; }
    }
}
