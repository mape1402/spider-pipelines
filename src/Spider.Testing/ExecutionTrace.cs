namespace Spider.Testing
{
    /// <summary>
    /// Stores the ordered execution trace recorded by Spider testing.
    /// </summary>
    public sealed class ExecutionTrace
    {
        private readonly List<ExecutionTraceEvent> _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionTrace"/> class.
        /// </summary>
        public ExecutionTrace()
        {
            _events = new List<ExecutionTraceEvent>();
        }

        /// <summary>
        /// Gets the recorded events in execution order.
        /// </summary>
        public IReadOnlyList<ExecutionTraceEvent> Events => _events;

        /// <summary>
        /// Gets a transaction-focused view of this trace.
        /// </summary>
        public TransactionTrace Transaction => new TransactionTrace(this, null);

        /// <summary>
        /// Removes all recorded events.
        /// </summary>
        public void Clear()
            => _events.Clear();

        /// <summary>
        /// Records an execution event.
        /// </summary>
        /// <param name="name">The logical event name.</param>
        /// <param name="operation">The operation recorded for the event.</param>
        /// <param name="requestType">The request type associated with the event.</param>
        /// <param name="exception">The exception associated with the event, when one exists.</param>
        /// <param name="metadata">Additional event metadata.</param>
        public void Add(
            string name,
            string operation,
            Type requestType = null,
            Exception exception = null,
            IReadOnlyDictionary<string, object> metadata = null)
            => _events.Add(new ExecutionTraceEvent(name, operation, requestType, exception, metadata));

        /// <summary>
        /// Creates a transaction-focused view for a specific boundary name.
        /// </summary>
        /// <param name="boundaryName">The transaction boundary name.</param>
        /// <returns>A transaction-focused trace.</returns>
        public TransactionTrace ForTransaction(string boundaryName)
            => new TransactionTrace(this, boundaryName);
    }
}
