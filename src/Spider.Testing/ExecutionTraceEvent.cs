namespace Spider.Testing
{
    /// <summary>
    /// Represents a single recorded Spider testing execution event.
    /// </summary>
    public sealed class ExecutionTraceEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionTraceEvent"/> class.
        /// </summary>
        /// <param name="name">The logical event name.</param>
        /// <param name="operation">The operation recorded for the event.</param>
        /// <param name="requestType">The request type associated with the event.</param>
        /// <param name="exception">The exception associated with the event, when one exists.</param>
        /// <param name="metadata">Additional event metadata.</param>
        public ExecutionTraceEvent(
            string name,
            string operation,
            Type requestType,
            Exception exception,
            IReadOnlyDictionary<string, object> metadata)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            RequestType = requestType;
            Exception = exception;
            Metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the logical event name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the operation recorded for the event.
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Gets the request type associated with the event.
        /// </summary>
        public Type RequestType { get; }

        /// <summary>
        /// Gets the exception associated with the event, when one exists.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets additional event metadata.
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata { get; }

        /// <inheritdoc/>
        public override string ToString()
            => $"{Name}:{Operation}";
    }
}
