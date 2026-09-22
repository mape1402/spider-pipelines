namespace Spider.Pipelines.Flows.Internals
{
    /// <summary>
    /// Describes a composed flow step for runtime tracing.
    /// </summary>
    internal sealed class FlowStepDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowStepDescriptor"/> class.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="operation">The operation name.</param>
        /// <param name="componentKind">The component kind.</param>
        /// <param name="inputType">The input type.</param>
        /// <param name="outputType">The output type.</param>
        /// <param name="metadata">The descriptive metadata.</param>
        public FlowStepDescriptor(
            string displayName,
            string operation,
            string componentKind,
            Type inputType,
            Type outputType,
            IReadOnlyDictionary<string, string> metadata = null)
        {
            Metadata = metadata == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(metadata, StringComparer.Ordinal);
            Tags = CreateTags(Metadata);
            DisplayName = ResolveDisplayName(displayName, Metadata);
            Operation = operation;
            ComponentKind = componentKind;
            InputType = inputType;
            OutputType = outputType;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the operation name.
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Gets the component kind.
        /// </summary>
        public string ComponentKind { get; }

        /// <summary>
        /// Gets the input type.
        /// </summary>
        public Type InputType { get; }

        /// <summary>
        /// Gets the output type.
        /// </summary>
        public Type OutputType { get; }

        /// <summary>
        /// Gets the step tags.
        /// </summary>
        public IReadOnlyDictionary<string, string> Tags { get; }

        /// <summary>
        /// Gets the step metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// Creates a descriptor from a delegate.
        /// </summary>
        /// <param name="delegate">The step delegate.</param>
        /// <param name="operation">The operation name.</param>
        /// <param name="componentKind">The component kind.</param>
        /// <param name="inputType">The input type.</param>
        /// <param name="outputType">The output type.</param>
        /// <param name="metadata">The descriptive metadata.</param>
        /// <returns>The step descriptor.</returns>
        public static FlowStepDescriptor FromDelegate(
            Delegate @delegate,
            string operation,
            string componentKind,
            Type inputType,
            Type outputType,
            IReadOnlyDictionary<string, string> metadata = null)
            => new(
                CleanName(@delegate?.Method?.Name) ?? operation,
                operation,
                componentKind,
                inputType,
                outputType,
                metadata);

        /// <summary>
        /// Creates trace tags from flow metadata.
        /// </summary>
        /// <param name="metadata">The metadata that may contain comma-separated tags.</param>
        /// <returns>The trace tags.</returns>
        public static IReadOnlyDictionary<string, string> CreateTags(IReadOnlyDictionary<string, string> metadata)
        {
            if (metadata == null ||
                !metadata.TryGetValue("tags", out var value) ||
                string.IsNullOrWhiteSpace(value))
            {
                return new Dictionary<string, string>();
            }

            return value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(tag => tag, tag => tag, StringComparer.Ordinal);
        }

        private static string ResolveDisplayName(
            string fallback,
            IReadOnlyDictionary<string, string> metadata)
        {
            if (metadata != null &&
                metadata.TryGetValue("name", out var name) &&
                !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return fallback;
        }

        private static string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return name.Contains("<", StringComparison.Ordinal)
                ? "lambda"
                : name;
        }
    }
}
