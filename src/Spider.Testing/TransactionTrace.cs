namespace Spider.Testing
{
    /// <summary>
    /// Provides a transaction-focused view over an execution trace.
    /// </summary>
    public sealed class TransactionTrace
    {
        private readonly ExecutionTrace _trace;
        private readonly string _boundaryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTrace"/> class.
        /// </summary>
        /// <param name="trace">The source execution trace.</param>
        /// <param name="boundaryName">An optional transaction boundary name.</param>
        public TransactionTrace(ExecutionTrace trace, string boundaryName)
        {
            _trace = trace ?? throw new ArgumentNullException(nameof(trace));
            _boundaryName = boundaryName;
        }

        /// <summary>
        /// Gets the transaction events.
        /// </summary>
        public IReadOnlyList<ExecutionTraceEvent> Events
            => _trace.Events.Where(IsTransactionEvent).ToArray();

        private bool IsTransactionEvent(ExecutionTraceEvent traceEvent)
        {
            if (!string.IsNullOrWhiteSpace(_boundaryName))
                return string.Equals(traceEvent.Name, _boundaryName, StringComparison.Ordinal);

            return traceEvent.Name.IndexOf("Transaction", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
