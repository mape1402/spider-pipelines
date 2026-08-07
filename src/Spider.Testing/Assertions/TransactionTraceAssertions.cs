namespace Spider.Testing.Assertions
{
    /// <summary>
    /// Provides assertion helpers for transaction traces.
    /// </summary>
    public static class TransactionTraceAssertions
    {
        /// <summary>
        /// Asserts that a transaction began.
        /// </summary>
        /// <param name="trace">The transaction trace.</param>
        public static void ShouldBegin(this TransactionTrace trace)
            => ShouldContainOperation(trace, "Begin");

        /// <summary>
        /// Asserts that a transaction committed.
        /// </summary>
        /// <param name="trace">The transaction trace.</param>
        public static void ShouldCommit(this TransactionTrace trace)
            => ShouldContainOperation(trace, "Commit");

        /// <summary>
        /// Asserts that a transaction rolled back.
        /// </summary>
        /// <param name="trace">The transaction trace.</param>
        public static void ShouldRollback(this TransactionTrace trace)
            => ShouldContainOperation(trace, "Rollback");

        private static void ShouldContainOperation(TransactionTrace trace, string operation)
        {
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            if (!trace.Events.Any(traceEvent => string.Equals(traceEvent.Operation, operation, StringComparison.Ordinal)))
                throw new SpiderTraceAssertionException($"Expected transaction trace to contain '{operation}'.");
        }
    }
}
