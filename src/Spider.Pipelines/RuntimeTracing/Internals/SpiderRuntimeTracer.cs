namespace Spider.Pipelines.RuntimeTracing.Internals
{
    using System.Threading.Channels;

    /// <summary>
    /// Emits runtime trace events through a bounded queue and best-effort dispatcher.
    /// </summary>
    internal sealed class SpiderRuntimeTracer : ISpiderRuntimeTracer, ISpiderTraceDispatcher, IDisposable, IAsyncDisposable
    {
        private readonly SpiderRuntimeTracingOptions _options;
        private readonly ISpiderTraceContextAccessor _contextAccessor;
        private readonly ISpiderTraceWriter _writer;
        private readonly ISpiderTraceLiveStream _liveStream;
        private readonly IReadOnlyList<ISpiderTraceSink> _sinks;
        private readonly IReadOnlyList<ISpiderTraceObserver> _observers;
        private readonly Channel<SpiderTraceEvent> _channel;
        private readonly CancellationTokenSource _shutdown = new();
        private readonly Task _dispatcher;
        private long _pendingEvents;
        private long _droppedEvents;
        private long _sampleCounter;
        private int _disposed;
        private readonly Dictionary<string, SpanState> _spans = new(StringComparer.Ordinal);
        private readonly object _sync = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderRuntimeTracer"/> class.
        /// </summary>
        /// <param name="options">The runtime tracing options.</param>
        /// <param name="contextAccessor">The trace context accessor.</param>
        /// <param name="writer">The trace writer.</param>
        /// <param name="liveStream">The live trace stream.</param>
        /// <param name="sinks">The configured trace sinks.</param>
        /// <param name="observers">The configured span observers.</param>
        public SpiderRuntimeTracer(
            SpiderRuntimeTracingOptions options,
            ISpiderTraceContextAccessor contextAccessor,
            ISpiderTraceWriter writer,
            ISpiderTraceLiveStream liveStream,
            IEnumerable<ISpiderTraceSink> sinks,
            IEnumerable<ISpiderTraceObserver> observers)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _liveStream = liveStream ?? throw new ArgumentNullException(nameof(liveStream));
            _sinks = (sinks ?? Enumerable.Empty<ISpiderTraceSink>()).ToArray();
            _observers = (observers ?? Enumerable.Empty<ISpiderTraceObserver>()).ToArray();
            _channel = Channel.CreateBounded<SpiderTraceEvent>(new BoundedChannelOptions(Math.Max(1, _options.QueueCapacity))
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
            _dispatcher = Task.Run(DispatchAsync);
        }

        /// <inheritdoc/>
        public bool IsEnabled => _options.Verbosity != SpiderTraceVerbosity.Off;

        /// <inheritdoc/>
        public long DroppedEventCount => Interlocked.Read(ref _droppedEvents);

        /// <inheritdoc/>
        public ValueTask<SpiderTraceScope> StartSpanAsync(
            SpiderTraceSpanDefinition definition,
            CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return new ValueTask<SpiderTraceScope>((SpiderTraceScope)null);

            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            var parent = _contextAccessor.Current;
            var traceId = parent?.TraceId ?? CreateId();
            var spanId = CreateId();
            var context = new SpiderTraceContext(traceId, spanId, parent?.SpanId, parent);
            var now = DateTimeOffset.UtcNow;

            var state = new SpanState
            {
                Context = context,
                Definition = definition,
                StartedAt = now
            };

            lock (_sync)
                _spans[spanId] = state;

            _contextAccessor.Current = context;

            var span = CreateSpan(state, SpiderTraceStatus.Running, null, null);
            NotifyStarted(span, cancellationToken);
            var scope = new SpiderTraceScope(this, context);
            var enqueue = AddEventAsync(CreateEvent(state, definition.StartedKind, SpiderTraceStatus.Running, now, null, null), cancellationToken);
            return enqueue.IsCompletedSuccessfully
                ? new ValueTask<SpiderTraceScope>(scope)
                : AwaitStartedEventAsync(enqueue, scope);
        }

        /// <inheritdoc/>
        public ValueTask AddEventAsync(
            SpiderTraceEvent traceEvent,
            CancellationToken cancellationToken = default)
        {
            if (!IsEnabled || traceEvent == null)
                return ValueTask.CompletedTask;

            var current = _contextAccessor.Current;
            if (current != null)
            {
                traceEvent.TraceId ??= current.TraceId;
                traceEvent.SpanId ??= current.SpanId;
                traceEvent.ParentSpanId ??= current.ParentSpanId;
            }

            traceEvent.Timestamp = traceEvent.Timestamp == default
                ? DateTimeOffset.UtcNow
                : traceEvent.Timestamp;

            return EnqueueAsync(traceEvent, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (Interlocked.Read(ref _pendingEvents) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(10, cancellationToken);
            }
        }

        internal ValueTask CompleteSpanAsync(
            SpiderTraceContext context,
            SpiderTraceStatus status,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (context == null)
                return ValueTask.CompletedTask;

            SpanState state;
            lock (_sync)
            {
                _spans.TryGetValue(context.SpanId, out state);
                _spans.Remove(context.SpanId);
            }

            _contextAccessor.Current = context.Previous;

            if (state == null)
                return ValueTask.CompletedTask;

            var completedAt = DateTimeOffset.UtcNow;
            var duration = completedAt - state.StartedAt;
            var kind = status switch
            {
                SpiderTraceStatus.Cancelled => state.Definition.CancelledKind,
                SpiderTraceStatus.Faulted => state.Definition.FaultedKind,
                _ => state.Definition.CompletedKind
            };

            var span = CreateSpan(state, status, completedAt, exception);
            NotifyCompleted(span, cancellationToken);
            return AddEventAsync(CreateEvent(state, kind, status, completedAt, duration, exception), cancellationToken);
        }

        private static async ValueTask<SpiderTraceScope> AwaitStartedEventAsync(
            ValueTask enqueue,
            SpiderTraceScope scope)
        {
            await enqueue.ConfigureAwait(false);
            return scope;
        }

        /// <inheritdoc/>
        public void Dispose()
            => DisposeAsync().AsTask().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            _channel.Writer.TryComplete();
            _shutdown.Cancel();

            try
            {
                await _dispatcher.ConfigureAwait(false);
            }
            catch
            {
                // Dispatcher failures are best-effort diagnostics and must not surface during disposal.
            }

            _shutdown.Dispose();
        }

        private static string CreateId()
            => Guid.NewGuid().ToString("N");

        private static string TypeName(Type type)
            => type == null ? null : type.FullName;

        private SpiderTraceSpan CreateSpan(
            SpanState state,
            SpiderTraceStatus status,
            DateTimeOffset? completedAt,
            Exception exception)
            => new()
            {
                TraceId = state.Context.TraceId,
                SpanId = state.Context.SpanId,
                ParentSpanId = state.Context.ParentSpanId,
                ComponentId = state.Definition.ComponentId,
                ComponentKind = state.Definition.ComponentKind,
                DisplayName = state.Definition.DisplayName,
                Operation = state.Definition.Operation,
                Status = status,
                StartedAt = state.StartedAt,
                CompletedAt = completedAt,
                Duration = completedAt.HasValue ? completedAt.Value - state.StartedAt : null,
                InputType = TypeName(state.Definition.InputType),
                OutputType = TypeName(state.Definition.OutputType),
                Exception = SpiderTraceException.FromException(exception),
                Tags = state.Definition.Tags,
                Metadata = state.Definition.Metadata
            };

        private SpiderTraceEvent CreateEvent(
            SpanState state,
            SpiderTraceEventKind kind,
            SpiderTraceStatus status,
            DateTimeOffset timestamp,
            TimeSpan? duration,
            Exception exception)
            => new()
            {
                TraceId = state.Context.TraceId,
                SpanId = state.Context.SpanId,
                ParentSpanId = state.Context.ParentSpanId,
                ComponentId = state.Definition.ComponentId,
                ComponentKind = state.Definition.ComponentKind,
                DisplayName = state.Definition.DisplayName,
                Operation = state.Definition.Operation,
                Kind = kind,
                Status = status,
                Timestamp = timestamp,
                Duration = duration,
                InputType = TypeName(state.Definition.InputType),
                OutputType = TypeName(state.Definition.OutputType),
                Exception = SpiderTraceException.FromException(exception),
                Tags = state.Definition.Tags,
                Metadata = state.Definition.Metadata
            };

        private ValueTask EnqueueAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
        {
            if (_options.Backpressure == SpiderTraceBackpressure.Block)
            {
                Interlocked.Increment(ref _pendingEvents);
                return AwaitWriteAsync(traceEvent, cancellationToken);
            }

            if (_options.Backpressure == SpiderTraceBackpressure.Sample
                && Interlocked.Increment(ref _sampleCounter) % 2 == 0)
            {
                DropEvent();
                return ValueTask.CompletedTask;
            }

            if (_channel.Writer.TryWrite(traceEvent))
            {
                Interlocked.Increment(ref _pendingEvents);
                return ValueTask.CompletedTask;
            }

            DropEvent();
            return ValueTask.CompletedTask;
        }

        private async ValueTask AwaitWriteAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
        {
            try
            {
                await _channel.Writer.WriteAsync(traceEvent, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                Interlocked.Decrement(ref _pendingEvents);
                DropEvent();
            }
        }

        private void DropEvent()
            => Interlocked.Increment(ref _droppedEvents);

        private async Task DispatchAsync()
        {
            await foreach (var traceEvent in _channel.Reader.ReadAllAsync(_shutdown.Token).ConfigureAwait(false))
            {
                try
                {
                    await SafeAppendAsync(traceEvent, _shutdown.Token).ConfigureAwait(false);
                    await SafePublishAsync(traceEvent, _shutdown.Token).ConfigureAwait(false);

                    foreach (var sink in _sinks)
                        await SafeSinkAsync(sink, traceEvent, _shutdown.Token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _pendingEvents);
                }
            }
        }

        private async ValueTask SafeAppendAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
        {
            try
            {
                await _writer.AppendAsync(traceEvent, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Runtime tracing is best-effort and must never break business execution.
            }
        }

        private async ValueTask SafePublishAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
        {
            try
            {
                await _liveStream.PublishAsync(traceEvent, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Runtime tracing is best-effort and must never break business execution.
            }
        }

        private static async ValueTask SafeSinkAsync(
            ISpiderTraceSink sink,
            SpiderTraceEvent traceEvent,
            CancellationToken cancellationToken)
        {
            try
            {
                await sink.WriteAsync(traceEvent, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Runtime tracing is best-effort and must never break business execution.
            }
        }

        private void NotifyStarted(SpiderTraceSpan span, CancellationToken cancellationToken)
            => _ = Task.Run(() => NotifyStartedAsync(span, cancellationToken), CancellationToken.None);

        private void NotifyCompleted(SpiderTraceSpan span, CancellationToken cancellationToken)
            => _ = Task.Run(() => NotifyCompletedAsync(span, cancellationToken), CancellationToken.None);

        private async Task NotifyStartedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    await observer.OnSpanStartedAsync(span, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // Runtime tracing observers are best-effort.
                }
            }
        }

        private async Task NotifyCompletedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    await observer.OnSpanCompletedAsync(span, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // Runtime tracing observers are best-effort.
                }
            }
        }

        private sealed class SpanState
        {
            public SpiderTraceContext Context { get; set; }

            public SpiderTraceSpanDefinition Definition { get; set; }

            public DateTimeOffset StartedAt { get; set; }
        }
    }
}
