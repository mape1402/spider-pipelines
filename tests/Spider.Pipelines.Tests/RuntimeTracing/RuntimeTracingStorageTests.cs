using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.RuntimeTracing;
using Spider.Pipelines.RuntimeTracing.Internals;
using Spider.Pipelines.RuntimeTracing.Stores;

namespace Spider.Pipelines.Tests.RuntimeTracing
{
    public sealed class RuntimeTracingStorageTests
    {
        [Fact]
        public void SpiderRuntimeTracingBuilder_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var services = new ServiceCollection();
            var options = new SpiderRuntimeTracingOptions();

            Assert.Throws<ArgumentNullException>(() => new SpiderRuntimeTracingBuilder(null, options));
            Assert.Throws<ArgumentNullException>(() => new SpiderRuntimeTracingBuilder(services, null));

            var builder = new SpiderRuntimeTracingBuilder(services, options);
            builder.Verbosity = SpiderTraceVerbosity.Diagnostic;
            builder.QueueCapacity = 12;
            builder.Backpressure = SpiderTraceBackpressure.Block;

            Assert.Equal(SpiderTraceVerbosity.Diagnostic, options.Verbosity);
            Assert.Equal(12, options.QueueCapacity);
            Assert.Equal(SpiderTraceBackpressure.Block, options.Backpressure);
            Assert.Throws<ArgumentNullException>(() => builder.UseStore(null));
        }

        [Fact]
        public void SpiderRuntimeTracingBuilder_WhenStoreAndExtensionsAreConfigured_ShouldRegisterServices()
        {
            var services = new ServiceCollection();
            var builder = new SpiderRuntimeTracingBuilder(services, new SpiderRuntimeTracingOptions());

            builder
                .UseInMemoryStore(options =>
                {
                    options.MaxTraces = 7;
                    options.MaxEventsPerTrace = 8;
                    options.TraceTtl = TimeSpan.FromSeconds(9);
                })
                .AddSink<RecordingTraceSink>()
                .AddObserver<RecordingTraceObserver>();

            using var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<InMemorySpiderTraceStoreOptions>();
            Assert.Equal(7, options.MaxTraces);
            Assert.Equal(8, options.MaxEventsPerTrace);
            Assert.Equal(TimeSpan.FromSeconds(9), options.TraceTtl);
            Assert.IsType<InMemorySpiderTraceStore>(provider.GetRequiredService<ISpiderTraceStore>());
            Assert.IsType<RecordingTraceSink>(provider.GetRequiredService<ISpiderTraceSink>());
            Assert.IsType<RecordingTraceObserver>(provider.GetRequiredService<ISpiderTraceObserver>());
        }

        [Fact]
        public void SpiderRuntimeTracingBuilder_WhenFactoryStoreIsConfigured_ShouldUseFactory()
        {
            var services = new ServiceCollection();
            var builder = new SpiderRuntimeTracingBuilder(services, new SpiderRuntimeTracingOptions());

            builder.UseStore(_ => new RecordingTraceStore());

            using var provider = services.BuildServiceProvider();

            Assert.IsType<RecordingTraceStore>(provider.GetRequiredService<ISpiderTraceStore>());
            Assert.Same(
                provider.GetRequiredService<ISpiderTraceStore>(),
                provider.GetRequiredService<ISpiderTraceWriter>());
            Assert.Same(
                provider.GetRequiredService<ISpiderTraceStore>(),
                provider.GetRequiredService<ISpiderTraceReader>());
        }

        [Fact]
        public void InMemorySpiderTraceStore_WhenArgumentsAreInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new InMemorySpiderTraceStore(null));

            var store = CreateStore();
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            Assert.ThrowsAsync<ArgumentNullException>(() => store.AppendAsync(null, CancellationToken.None).AsTask());
            Assert.ThrowsAsync<OperationCanceledException>(() => store.AppendAsync(CreateEvent("cancelled"), cancelled.Token).AsTask());
            Assert.ThrowsAsync<ArgumentException>(() => store.GetAsync(" ", CancellationToken.None).AsTask());
            Assert.ThrowsAsync<OperationCanceledException>(() => store.GetAsync("trace", cancelled.Token).AsTask());
        }

        [Fact]
        public async Task InMemorySpiderTraceStore_WhenEventsAreStored_ShouldReturnTraceAndSummary()
        {
            var store = CreateStore();
            var started = DateTimeOffset.UtcNow.AddMilliseconds(-25);
            var completed = DateTimeOffset.UtcNow;

            await store.AppendAsync(CreateEvent("trace-a", started, SpiderTraceStatus.Running, SpiderTraceEventKind.FlowStarted, "Root flow"), CancellationToken.None);
            await store.AppendAsync(CreateEvent("trace-a", completed, SpiderTraceStatus.Completed, SpiderTraceEventKind.FlowCompleted, "Root flow"), CancellationToken.None);

            var trace = await store.GetAsync("trace-a", CancellationToken.None);
            var summaries = await QueryAsync(store, new SpiderTraceQuery { SearchText = "root", Status = SpiderTraceStatus.Completed, Limit = 0 });

            Assert.Equal("trace-a", trace.TraceId);
            Assert.Equal(SpiderTraceStatus.Completed, trace.Status);
            Assert.NotNull(trace.CompletedAt);
            Assert.NotNull(trace.Duration);
            Assert.Equal(2, trace.Events.Count);
            var summary = Assert.Single(summaries);
            Assert.Equal("trace-a", summary.TraceId);
            Assert.Equal("Root flow", summary.RootDisplayName);
            Assert.Equal("InputModel", summary.RequestType);
            Assert.Equal("OutputModel", summary.ResponseType);
            Assert.Equal(2, summary.EventCount);
        }

        [Fact]
        public async Task InMemorySpiderTraceStore_WhenQueryFiltersDoNotMatch_ShouldReturnNoSummaries()
        {
            var store = CreateStore();
            await store.AppendAsync(CreateEvent("trace-a", displayName: "Checkout flow"), CancellationToken.None);

            Assert.Empty(await QueryAsync(store, new SpiderTraceQuery { Status = SpiderTraceStatus.Faulted }));
            Assert.Empty(await QueryAsync(store, new SpiderTraceQuery { SearchText = "missing" }));
            Assert.Single(await QueryAsync(store, new SpiderTraceQuery { SearchText = "inputmodel" }));
            Assert.Single(await QueryAsync(store, new SpiderTraceQuery { SearchText = "outputmodel" }));
            Assert.Single(await QueryAsync(store, null));
        }

        [Fact]
        public async Task InMemorySpiderTraceStore_WhenLimitsAreExceeded_ShouldPruneOldestData()
        {
            var store = CreateStore(new InMemorySpiderTraceStoreOptions
            {
                MaxTraces = 2,
                MaxEventsPerTrace = 2,
                TraceTtl = TimeSpan.Zero
            });

            await store.AppendAsync(CreateEvent("trace-a", DateTimeOffset.UtcNow.AddMinutes(-3)), CancellationToken.None);
            await store.AppendAsync(CreateEvent("trace-b", DateTimeOffset.UtcNow.AddMinutes(-2)), CancellationToken.None);
            await store.AppendAsync(CreateEvent("trace-c", DateTimeOffset.UtcNow.AddMinutes(-1)), CancellationToken.None);
            await store.AppendAsync(CreateEvent("trace-c", kind: SpiderTraceEventKind.EventDropped), CancellationToken.None);
            await store.AppendAsync(CreateEvent("trace-c", status: SpiderTraceStatus.Completed, kind: SpiderTraceEventKind.FlowCompleted), CancellationToken.None);

            Assert.Null(await store.GetAsync("trace-a", CancellationToken.None));

            var trace = await store.GetAsync("trace-c", CancellationToken.None);
            var summary = Assert.Single(await QueryAsync(store, new SpiderTraceQuery { SearchText = "trace-c" }));
            Assert.Equal(2, trace.Events.Count);
            Assert.Equal(2, summary.DroppedEventCount);
        }

        [Fact]
        public async Task InMemorySpiderTraceStore_WhenTraceIsExpired_ShouldPruneIt()
        {
            var store = CreateStore(new InMemorySpiderTraceStoreOptions
            {
                MaxTraces = 10,
                MaxEventsPerTrace = 10,
                TraceTtl = TimeSpan.FromSeconds(1)
            });

            await store.AppendAsync(CreateEvent("old", DateTimeOffset.UtcNow.AddMinutes(-1)), CancellationToken.None);
            await store.AppendAsync(CreateEvent("new", DateTimeOffset.UtcNow), CancellationToken.None);

            Assert.Null(await store.GetAsync("old", CancellationToken.None));
            Assert.NotNull(await store.GetAsync("new", CancellationToken.None));
        }

        [Fact]
        public async Task InMemorySpiderTraceLiveStream_WhenPublished_ShouldFilterEventsForSubscribers()
        {
            var stream = new InMemorySpiderTraceLiveStream();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await using var subscription = stream
                .SubscribeAsync(new SpiderTraceQuery { Status = SpiderTraceStatus.Completed, SearchText = "target" }, timeout.Token)
                .GetAsyncEnumerator(timeout.Token);

            var next = subscription.MoveNextAsync().AsTask();

            await stream.PublishAsync(CreateEvent("ignored", status: SpiderTraceStatus.Running, displayName: "target"), CancellationToken.None);
            await stream.PublishAsync(CreateEvent("ignored", status: SpiderTraceStatus.Completed, displayName: "other"), CancellationToken.None);
            await stream.PublishAsync(CreateEvent("target-trace", status: SpiderTraceStatus.Completed, displayName: "target"), CancellationToken.None);

            Assert.True(await next);
            Assert.Equal("target-trace", subscription.Current.TraceId);
        }

        [Fact]
        public async Task InMemorySpiderTraceLiveStream_WhenQueryIsNull_ShouldPublishAllEvents()
        {
            var stream = new InMemorySpiderTraceLiveStream();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await using var subscription = stream
                .SubscribeAsync(null, timeout.Token)
                .GetAsyncEnumerator(timeout.Token);

            var next = subscription.MoveNextAsync().AsTask();
            await stream.PublishAsync(CreateEvent("trace-a"), CancellationToken.None);

            Assert.True(await next);
            Assert.Equal("trace-a", subscription.Current.TraceId);
        }

        [Fact]
        public async Task InMemorySpiderTraceLiveStream_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var stream = new InMemorySpiderTraceLiveStream();
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await Assert.ThrowsAsync<ArgumentNullException>(() => stream.PublishAsync(null, CancellationToken.None).AsTask());
            await Assert.ThrowsAsync<OperationCanceledException>(() => stream.PublishAsync(CreateEvent("trace-a"), cancelled.Token).AsTask());
        }

        private static InMemorySpiderTraceStore CreateStore(InMemorySpiderTraceStoreOptions options = null)
            => new(options ?? new InMemorySpiderTraceStoreOptions
            {
                MaxTraces = 10,
                MaxEventsPerTrace = 10,
                TraceTtl = TimeSpan.Zero
            });

        private static async Task<IReadOnlyList<SpiderTraceSummary>> QueryAsync(
            InMemorySpiderTraceStore store,
            SpiderTraceQuery query)
        {
            var summaries = new List<SpiderTraceSummary>();
            await foreach (var summary in store.QueryAsync(query, CancellationToken.None))
                summaries.Add(summary);

            return summaries;
        }

        private static SpiderTraceEvent CreateEvent(
            string traceId,
            DateTimeOffset? timestamp = null,
            SpiderTraceStatus status = SpiderTraceStatus.Running,
            SpiderTraceEventKind kind = SpiderTraceEventKind.FlowStarted,
            string displayName = "Display")
            => new()
            {
                TraceId = traceId,
                SpanId = $"{traceId}-span",
                ComponentId = $"{traceId}-component",
                ComponentKind = "Flow",
                DisplayName = displayName,
                Operation = "Flow",
                Kind = kind,
                Status = status,
                Timestamp = timestamp ?? DateTimeOffset.UtcNow,
                InputType = "InputModel",
                OutputType = "OutputModel"
            };

        private sealed class RecordingTraceStore : ISpiderTraceStore
        {
            private readonly InMemorySpiderTraceStore _inner = CreateStore();

            public ValueTask AppendAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
                => _inner.AppendAsync(traceEvent, cancellationToken);

            public ValueTask<SpiderTrace> GetAsync(string traceId, CancellationToken cancellationToken)
                => _inner.GetAsync(traceId, cancellationToken);

            public IAsyncEnumerable<SpiderTraceSummary> QueryAsync(SpiderTraceQuery query, CancellationToken cancellationToken)
                => _inner.QueryAsync(query, cancellationToken);
        }

        private sealed class RecordingTraceSink : ISpiderTraceSink
        {
            public ValueTask WriteAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
                => ValueTask.CompletedTask;
        }

        private sealed class RecordingTraceObserver : ISpiderTraceObserver
        {
            public ValueTask OnSpanStartedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
                => ValueTask.CompletedTask;

            public ValueTask OnSpanCompletedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
                => ValueTask.CompletedTask;
        }
    }
}
