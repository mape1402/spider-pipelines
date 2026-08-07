namespace Spider.Testing.Assertions
{
    /// <summary>
    /// Provides assertion helpers for Spider execution traces.
    /// </summary>
    public static class ExecutionTraceAssertions
    {
        /// <summary>
        /// Asserts that a trace contains an event with the specified target name.
        /// </summary>
        /// <param name="trace">The execution trace.</param>
        /// <param name="target">The expected event or boundary name.</param>
        public static void ShouldContain(this ExecutionTrace trace, string target)
        {
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Assertion target cannot be empty.", nameof(target));

            if (!trace.Events.Any(traceEvent => Matches(traceEvent, target)))
                throw new SpiderTraceAssertionException($"Expected trace to contain '{target}'.");
        }

        /// <summary>
        /// Asserts that a trace does not contain an event with the specified target name.
        /// </summary>
        /// <param name="trace">The execution trace.</param>
        /// <param name="target">The unexpected event or boundary name.</param>
        public static void ShouldNotContain(this ExecutionTrace trace, string target)
        {
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Assertion target cannot be empty.", nameof(target));

            if (trace.Events.Any(traceEvent => Matches(traceEvent, target)))
                throw new SpiderTraceAssertionException($"Expected trace not to contain '{target}'.");
        }

        /// <summary>
        /// Asserts that one trace target appears before another.
        /// </summary>
        /// <param name="trace">The execution trace.</param>
        /// <param name="first">The event or boundary name expected first.</param>
        /// <param name="second">The event or boundary name expected second.</param>
        public static void ShouldRunBefore(this ExecutionTrace trace, string first, string second)
        {
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            var firstIndex = IndexOf(trace, first);
            var secondIndex = IndexOf(trace, second);

            if (firstIndex < 0)
                throw new SpiderTraceAssertionException($"Expected trace to contain '{first}'.");

            if (secondIndex < 0)
                throw new SpiderTraceAssertionException($"Expected trace to contain '{second}'.");

            if (firstIndex >= secondIndex)
                throw new SpiderTraceAssertionException($"Expected '{first}' to run before '{second}'.");
        }

        /// <summary>
        /// Asserts that request metadata was recorded.
        /// </summary>
        /// <typeparam name="TRequest">The expected request type.</typeparam>
        /// <param name="trace">The execution trace.</param>
        public static void ShouldRecordRequest<TRequest>(this ExecutionTrace trace)
        {
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            if (!trace.Events.Any(traceEvent => traceEvent.RequestType == typeof(TRequest)))
                throw new SpiderTraceAssertionException($"Expected trace to record request type '{typeof(TRequest).Name}'.");
        }

        private static int IndexOf(ExecutionTrace trace, string target)
            => trace.Events
                .Select((traceEvent, index) => new { traceEvent, index })
                .Where(item => Matches(item.traceEvent, target))
                .Select(item => item.index)
                .DefaultIfEmpty(-1)
                .First();

        private static bool Matches(ExecutionTraceEvent traceEvent, string target)
            => string.Equals(traceEvent.Name, target, StringComparison.Ordinal)
               || string.Equals(traceEvent.Operation, target, StringComparison.Ordinal)
               || string.Equals(traceEvent.ToString(), target, StringComparison.Ordinal);
    }
}
