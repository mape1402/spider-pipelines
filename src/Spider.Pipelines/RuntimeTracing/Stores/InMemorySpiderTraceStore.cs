namespace Spider.Pipelines.RuntimeTracing.Stores
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Stores runtime traces in memory for local development and lightweight diagnostics.
    /// </summary>
    public sealed class InMemorySpiderTraceStore : ISpiderTraceStore
    {
        private readonly ConcurrentDictionary<string, TraceBuffer> _traces = new();
        private readonly InMemorySpiderTraceStoreOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySpiderTraceStore"/> class.
        /// </summary>
        /// <param name="options">The in-memory store options.</param>
        public InMemorySpiderTraceStore(InMemorySpiderTraceStoreOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public ValueTask AppendAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
        {
            if (traceEvent == null)
                throw new ArgumentNullException(nameof(traceEvent));

            cancellationToken.ThrowIfCancellationRequested();

            PruneExpired();

            var buffer = _traces.GetOrAdd(traceEvent.TraceId, _ => new TraceBuffer());
            buffer.Append(traceEvent, Math.Max(1, _options.MaxEventsPerTrace));
            PruneOverflow();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask<SpiderTrace> GetAsync(string traceId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(traceId))
                throw new ArgumentException("Trace id is required.", nameof(traceId));

            cancellationToken.ThrowIfCancellationRequested();
            PruneExpired();

            return new ValueTask<SpiderTrace>(
                _traces.TryGetValue(traceId, out var buffer)
                    ? buffer.ToTrace(traceId)
                    : null);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<SpiderTraceSummary> QueryAsync(
            SpiderTraceQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            query ??= new SpiderTraceQuery();
            cancellationToken.ThrowIfCancellationRequested();
            PruneExpired();

            var limit = query.Limit <= 0 ? 100 : query.Limit;
            var summaries = _traces
                .Select(item => item.Value.ToSummary(item.Key))
                .Where(summary => Matches(summary, query))
                .OrderByDescending(summary => summary.StartedAt)
                .Take(limit)
                .ToArray();

            foreach (var summary in summaries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return summary;
                await Task.Yield();
            }
        }

        private static bool Matches(SpiderTraceSummary summary, SpiderTraceQuery query)
        {
            if (query.Status.HasValue && summary.Status != query.Status.Value)
                return false;

            if (string.IsNullOrWhiteSpace(query.SearchText))
                return true;

            var search = query.SearchText.Trim();
            return Contains(summary.TraceId, search)
                || Contains(summary.RootDisplayName, search)
                || Contains(summary.RequestType, search)
                || Contains(summary.ResponseType, search);
        }

        private static bool Contains(string value, string search)
            => value != null && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private void PruneExpired()
        {
            if (_options.TraceTtl <= TimeSpan.Zero)
                return;

            var cutoff = DateTimeOffset.UtcNow.Subtract(_options.TraceTtl);
            foreach (var item in _traces.ToArray())
            {
                var summary = item.Value.ToSummary(item.Key);
                if (summary.StartedAt < cutoff)
                    _traces.TryRemove(item.Key, out _);
            }
        }

        private void PruneOverflow()
        {
            var maxTraces = Math.Max(1, _options.MaxTraces);
            var overflow = _traces.Count - maxTraces;
            if (overflow <= 0)
                return;

            foreach (var traceId in _traces
                .Select(item => item.Value.ToSummary(item.Key))
                .OrderBy(summary => summary.StartedAt)
                .Take(overflow)
                .Select(summary => summary.TraceId)
                .ToArray())
            {
                _traces.TryRemove(traceId, out _);
            }
        }

        private sealed class TraceBuffer
        {
            private readonly List<SpiderTraceEvent> _events = new();
            private int _droppedEvents;

            public void Append(SpiderTraceEvent traceEvent, int maxEvents)
            {
                lock (_events)
                {
                    if (_events.Count >= maxEvents)
                    {
                        _events.RemoveAt(0);
                        _droppedEvents++;
                    }

                    _events.Add(traceEvent);
                }
            }

            public SpiderTrace ToTrace(string traceId)
            {
                var events = Snapshot();
                var summary = ToSummary(traceId, events);
                return new SpiderTrace
                {
                    TraceId = traceId,
                    Status = summary.Status,
                    StartedAt = summary.StartedAt,
                    CompletedAt = summary.CompletedAt,
                    Duration = summary.Duration,
                    Events = events
                };
            }

            public SpiderTraceSummary ToSummary(string traceId)
                => ToSummary(traceId, Snapshot());

            private SpiderTraceSummary ToSummary(string traceId, IReadOnlyList<SpiderTraceEvent> events)
            {
                var first = events.FirstOrDefault();
                var last = events.LastOrDefault();
                var root = events.FirstOrDefault(item => string.IsNullOrEmpty(item.ParentSpanId)) ?? first;
                var startedAt = first?.Timestamp ?? DateTimeOffset.MinValue;
                var completedAt = IsTerminal(last?.Status) ? last?.Timestamp : null;

                return new SpiderTraceSummary
                {
                    TraceId = traceId,
                    RootComponentId = root?.ComponentId,
                    RootDisplayName = root?.DisplayName,
                    RequestType = root?.InputType,
                    ResponseType = root?.OutputType,
                    Status = last?.Status ?? SpiderTraceStatus.Running,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    Duration = completedAt.HasValue && startedAt != DateTimeOffset.MinValue
                        ? completedAt.Value - startedAt
                        : null,
                    EventCount = events.Count,
                    DroppedEventCount = _droppedEvents + events.Count(item => item.Kind == SpiderTraceEventKind.EventDropped)
                };
            }

            private IReadOnlyList<SpiderTraceEvent> Snapshot()
            {
                lock (_events)
                    return _events.ToArray();
            }

            private static bool IsTerminal(SpiderTraceStatus? status)
                => status == SpiderTraceStatus.Completed
                || status == SpiderTraceStatus.Faulted
                || status == SpiderTraceStatus.Cancelled;
        }
    }
}
