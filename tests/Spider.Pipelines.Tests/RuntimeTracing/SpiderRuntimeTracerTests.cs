using Spider.Pipelines.RuntimeTracing;
using Spider.Pipelines.RuntimeTracing.Internals;

namespace Spider.Pipelines.Tests.RuntimeTracing
{
    public sealed class SpiderRuntimeTracerTests
    {
        [Fact]
        public async Task StartSpanAsync_WhenTracingIsDisabled_ShouldReturnNullAndIgnoreEvents()
        {
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions { Verbosity = SpiderTraceVerbosity.Off },
                out var writer,
                out _,
                out _,
                out _);

            var scope = await tracer.StartSpanAsync(CreateDefinition());
            await tracer.AddEventAsync(new SpiderTraceEvent { DisplayName = "ignored" }, CancellationToken.None);
            await tracer.FlushAsync(CancellationToken.None);

            Assert.False(tracer.IsEnabled);
            Assert.Null(scope);
            Assert.Empty(writer.Events);
        }

        [Fact]
        public async Task StartSpanAsync_WhenDefinitionIsNull_ShouldThrow()
        {
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions(),
                out _,
                out _,
                out _,
                out _);

            await Assert.ThrowsAsync<ArgumentNullException>(() => tracer.StartSpanAsync(null).AsTask());
        }

        [Fact]
        public async Task SpanLifecycle_WhenCompletedFaultedAndCancelled_ShouldEmitTerminalEvents()
        {
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions(),
                out var writer,
                out _,
                out _,
                out var observer);

            var completed = await tracer.StartSpanAsync(CreateDefinition("completed"));
            Assert.False(string.IsNullOrWhiteSpace(completed.TraceId));
            Assert.False(string.IsNullOrWhiteSpace(completed.SpanId));
            await completed.CompleteAsync();
            await completed.CompleteAsync();

            var faulted = await tracer.StartSpanAsync(CreateDefinition("faulted"));
            await faulted.FaultAsync(new InvalidOperationException("faulted span"));
            await faulted.CancelAsync();

            var cancelled = await tracer.StartSpanAsync(CreateDefinition("cancelled"));
            await cancelled.CancelAsync();
            await cancelled.DisposeAsync();

            var disposed = await tracer.StartSpanAsync(CreateDefinition("disposed"));
            await disposed.DisposeAsync();

            await tracer.FlushAsync(CancellationToken.None);
            await observer.WaitForCompletionsAsync(4);

            Assert.Contains(writer.Events, item => item.Kind == SpiderTraceEventKind.TraceCompleted && item.DisplayName == "completed");
            Assert.Contains(writer.Events, item => item.Kind == SpiderTraceEventKind.TraceFaulted && item.Exception.Message == "faulted span");
            Assert.Contains(writer.Events, item => item.Kind == SpiderTraceEventKind.TraceCancelled && item.DisplayName == "cancelled");
            Assert.Contains(writer.Events, item => item.Kind == SpiderTraceEventKind.TraceCompleted && item.DisplayName == "disposed");
            Assert.Equal(4, observer.Completed.Count);
        }

        [Fact]
        public async Task AddEventAsync_WhenContextExists_ShouldPopulateMissingContextAndTimestamp()
        {
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions(),
                out var writer,
                out _,
                out _,
                out _);

            var scope = await tracer.StartSpanAsync(CreateDefinition("context"));

            await tracer.AddEventAsync(new SpiderTraceEvent
            {
                DisplayName = "custom",
                Kind = SpiderTraceEventKind.EventDropped
            });

            await scope.CompleteAsync();
            await tracer.FlushAsync(CancellationToken.None);

            var custom = Assert.Single(writer.Events, item => item.DisplayName == "custom");
            Assert.Equal(scope.TraceId, custom.TraceId);
            Assert.Equal(scope.SpanId, custom.SpanId);
            Assert.NotEqual(default, custom.Timestamp);
        }

        [Fact]
        public async Task AddEventAsync_WhenBackpressureSamplesEvents_ShouldDropEveryOtherEvent()
        {
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions { Backpressure = SpiderTraceBackpressure.Sample },
                out var writer,
                out _,
                out _,
                out _);

            await tracer.AddEventAsync(CreateEvent("first"));
            await tracer.AddEventAsync(CreateEvent("second"));
            await tracer.FlushAsync(CancellationToken.None);

            Assert.Equal(1, tracer.DroppedEventCount);
            Assert.Single(writer.Events);
            Assert.Equal("first", writer.Events[0].DisplayName);
        }

        [Fact]
        public async Task AddEventAsync_WhenQueueIsFull_ShouldDropNewestEvent()
        {
            var writer = new BlockingTraceWriter();
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions { QueueCapacity = 1, Backpressure = SpiderTraceBackpressure.DropNewest },
                writer,
                new RecordingLiveStream(),
                Array.Empty<ISpiderTraceSink>(),
                Array.Empty<ISpiderTraceObserver>());

            await tracer.AddEventAsync(CreateEvent("held"));
            await writer.WaitForFirstWriteAsync();
            await tracer.AddEventAsync(CreateEvent("queued"));
            await tracer.AddEventAsync(CreateEvent("dropped"));

            writer.Release();
            await tracer.FlushAsync(CancellationToken.None);

            Assert.True(tracer.DroppedEventCount >= 1);
            Assert.Contains(writer.Events, item => item.DisplayName == "held");
            Assert.Contains(writer.Events, item => item.DisplayName == "queued");
            Assert.DoesNotContain(writer.Events, item => item.DisplayName == "dropped");
        }

        [Fact]
        public async Task AddEventAsync_WhenBackpressureBlocksAndWriteIsCancelled_ShouldDropEvent()
        {
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions { Backpressure = SpiderTraceBackpressure.Block },
                out _,
                out _,
                out _,
                out _);
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await tracer.AddEventAsync(CreateEvent("cancelled"), cancelled.Token);

            Assert.Equal(1, tracer.DroppedEventCount);
        }

        [Fact]
        public async Task DispatchAsync_WhenOutputsThrow_ShouldKeepRuntimeTracingBestEffort()
        {
            await using var tracer = CreateTracer(
                new SpiderRuntimeTracingOptions(),
                new ThrowingTraceWriter(),
                new ThrowingLiveStream(),
                new ISpiderTraceSink[] { new ThrowingTraceSink() },
                new ISpiderTraceObserver[] { new ThrowingTraceObserver() });

            var scope = await tracer.StartSpanAsync(CreateDefinition("throwing"));
            await scope.CompleteAsync();
            await tracer.AddEventAsync(CreateEvent("also throwing"));
            await tracer.FlushAsync(CancellationToken.None);
            await Task.Delay(50);

            Assert.True(tracer.IsEnabled);
        }

        private static SpiderRuntimeTracer CreateTracer(
            SpiderRuntimeTracingOptions options,
            out RecordingTraceWriter writer,
            out RecordingLiveStream liveStream,
            out RecordingTraceSink sink,
            out RecordingTraceObserver observer)
        {
            writer = new RecordingTraceWriter();
            liveStream = new RecordingLiveStream();
            sink = new RecordingTraceSink();
            observer = new RecordingTraceObserver();
            return CreateTracer(
                options,
                writer,
                liveStream,
                new ISpiderTraceSink[] { sink },
                new ISpiderTraceObserver[] { observer });
        }

        private static SpiderRuntimeTracer CreateTracer(
            SpiderRuntimeTracingOptions options,
            ISpiderTraceWriter writer,
            ISpiderTraceLiveStream liveStream,
            IEnumerable<ISpiderTraceSink> sinks,
            IEnumerable<ISpiderTraceObserver> observers)
            => new(
                options,
                new SpiderTraceContextAccessor(),
                writer,
                liveStream,
                sinks,
                observers);

        private static SpiderTraceSpanDefinition CreateDefinition(string displayName = "span")
            => new()
            {
                ComponentId = $"component:{displayName}",
                ComponentKind = "Test",
                DisplayName = displayName,
                Operation = "Operation",
                InputType = typeof(TestInput),
                OutputType = typeof(TestOutput),
                Tags = new Dictionary<string, string> { ["tag"] = "tag-value" },
                Metadata = new Dictionary<string, string> { ["description"] = "Test span" }
            };

        private static SpiderTraceEvent CreateEvent(string displayName)
            => new()
            {
                TraceId = $"trace-{displayName}",
                SpanId = $"span-{displayName}",
                ComponentId = $"component-{displayName}",
                ComponentKind = "Test",
                DisplayName = displayName,
                Operation = "Operation",
                Kind = SpiderTraceEventKind.TraceStarted,
                Status = SpiderTraceStatus.Running,
                Timestamp = DateTimeOffset.UtcNow
            };

        private sealed record TestInput;

        private sealed record TestOutput;

        private class RecordingTraceWriter : ISpiderTraceWriter
        {
            public List<SpiderTraceEvent> Events { get; } = new();

            public virtual ValueTask AppendAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
            {
                Events.Add(traceEvent);
                return ValueTask.CompletedTask;
            }
        }

        private sealed class BlockingTraceWriter : RecordingTraceWriter
        {
            private readonly TaskCompletionSource _firstWriteStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public override async ValueTask AppendAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
            {
                _firstWriteStarted.TrySetResult();
                await _release.Task.WaitAsync(cancellationToken);
                await base.AppendAsync(traceEvent, cancellationToken);
            }

            public Task WaitForFirstWriteAsync()
                => _firstWriteStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

            public void Release()
                => _release.TrySetResult();
        }

        private sealed class ThrowingTraceWriter : ISpiderTraceWriter
        {
            public ValueTask AppendAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
                => throw new InvalidOperationException("writer failed");
        }

        private sealed class RecordingLiveStream : ISpiderTraceLiveStream
        {
            public List<SpiderTraceEvent> Events { get; } = new();

            public ValueTask PublishAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
            {
                Events.Add(traceEvent);
                return ValueTask.CompletedTask;
            }

            public async IAsyncEnumerable<SpiderTraceEvent> SubscribeAsync(
                SpiderTraceQuery query,
                [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
            {
                foreach (var traceEvent in Events)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return traceEvent;
                    await Task.Yield();
                }
            }
        }

        private sealed class ThrowingLiveStream : ISpiderTraceLiveStream
        {
            public ValueTask PublishAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
                => throw new InvalidOperationException("live stream failed");

            public async IAsyncEnumerable<SpiderTraceEvent> SubscribeAsync(
                SpiderTraceQuery query,
                [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                yield break;
            }
        }

        private sealed class RecordingTraceSink : ISpiderTraceSink
        {
            public List<SpiderTraceEvent> Events { get; } = new();

            public ValueTask WriteAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
            {
                Events.Add(traceEvent);
                return ValueTask.CompletedTask;
            }
        }

        private sealed class ThrowingTraceSink : ISpiderTraceSink
        {
            public ValueTask WriteAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
                => throw new InvalidOperationException("sink failed");
        }

        private sealed class RecordingTraceObserver : ISpiderTraceObserver
        {
            private readonly TaskCompletionSource _completionReached = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public List<SpiderTraceSpan> Started { get; } = new();

            public List<SpiderTraceSpan> Completed { get; } = new();

            public ValueTask OnSpanStartedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
            {
                Started.Add(span);
                return ValueTask.CompletedTask;
            }

            public ValueTask OnSpanCompletedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
            {
                Completed.Add(span);
                if (Completed.Count >= 4)
                    _completionReached.TrySetResult();

                return ValueTask.CompletedTask;
            }

            public Task WaitForCompletionsAsync(int count)
                => Completed.Count >= count
                    ? Task.CompletedTask
                    : _completionReached.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }

        private sealed class ThrowingTraceObserver : ISpiderTraceObserver
        {
            public ValueTask OnSpanStartedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
                => throw new InvalidOperationException("observer start failed");

            public ValueTask OnSpanCompletedAsync(SpiderTraceSpan span, CancellationToken cancellationToken)
                => throw new InvalidOperationException("observer complete failed");
        }
    }
}
