namespace Spider.Pipelines.RuntimeTracing.Internals
{
    using System.Collections.Concurrent;
    using System.Threading.Channels;

    /// <summary>
    /// Publishes runtime trace events to in-memory subscribers.
    /// </summary>
    internal sealed class InMemorySpiderTraceLiveStream : ISpiderTraceLiveStream
    {
        private readonly ConcurrentDictionary<Guid, Channel<SpiderTraceEvent>> _subscribers = new();

        /// <inheritdoc/>
        public ValueTask PublishAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
        {
            if (traceEvent == null)
                throw new ArgumentNullException(nameof(traceEvent));

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var subscriber in _subscribers.Values)
                subscriber.Writer.TryWrite(traceEvent);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<SpiderTraceEvent> SubscribeAsync(
            SpiderTraceQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var channel = Channel.CreateUnbounded<SpiderTraceEvent>();
            _subscribers[id] = channel;

            try
            {
                await foreach (var traceEvent in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    if (Matches(traceEvent, query))
                        yield return traceEvent;
                }
            }
            finally
            {
                _subscribers.TryRemove(id, out _);
            }
        }

        private static bool Matches(SpiderTraceEvent traceEvent, SpiderTraceQuery query)
        {
            if (query == null)
                return true;

            if (query.Status.HasValue && traceEvent.Status != query.Status.Value)
                return false;

            if (string.IsNullOrWhiteSpace(query.SearchText))
                return true;

            var search = query.SearchText.Trim();
            return Contains(traceEvent.TraceId, search)
                || Contains(traceEvent.DisplayName, search)
                || Contains(traceEvent.InputType, search)
                || Contains(traceEvent.OutputType, search);
        }

        private static bool Contains(string value, string search)
            => value != null && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
