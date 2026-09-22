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
        public FlowStepDescriptor(
            string displayName,
            string operation,
            string componentKind,
            Type inputType,
            Type outputType)
        {
            DisplayName = displayName;
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
        /// Creates a descriptor from a delegate.
        /// </summary>
        /// <param name="delegate">The step delegate.</param>
        /// <param name="operation">The operation name.</param>
        /// <param name="componentKind">The component kind.</param>
        /// <param name="inputType">The input type.</param>
        /// <param name="outputType">The output type.</param>
        /// <returns>The step descriptor.</returns>
        public static FlowStepDescriptor FromDelegate(
            Delegate @delegate,
            string operation,
            string componentKind,
            Type inputType,
            Type outputType)
            => new(
                CleanName(@delegate?.Method?.Name) ?? operation,
                operation,
                componentKind,
                inputType,
                outputType);

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
