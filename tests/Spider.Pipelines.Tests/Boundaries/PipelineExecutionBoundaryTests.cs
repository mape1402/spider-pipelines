using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Boundaries.Internals;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;
using Spider.Pipelines.RuntimeTracing;

namespace Spider.Pipelines.Tests.Boundaries
{
    /// <summary>
    /// Verifies provider-agnostic execution boundary behavior.
    /// </summary>
    public class PipelineExecutionBoundaryTests
    {
        /// <summary>
        /// Verifies that pipelines keep their existing behavior when no boundaries are registered.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenNoBoundariesAreRegistered_ShouldKeepCurrentBehavior()
        {
            var bridge = CreateBridge();

            var result = await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(6, result);
        }

        /// <summary>
        /// Verifies that boundaries remain active through every pipeline phase.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_ShouldKeepBoundaryActiveThroughPreTargetAndPost()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder =>
                {
                    builder
                        .PreProcess<string, int>((ctx, args) =>
                        {
                            log.Add(log.Active ? "pre-active" : "pre-inactive");
                            return Task.CompletedTask;
                        })
                        .OnSuccess<string, int>((ctx, args) =>
                        {
                            log.Add(log.Active ? "post-active" : "post-inactive");
                            return Task.CompletedTask;
                        });
                })
                .ExecuteAsync(service => (request, token) => service.HandleWithBoundaryCheckAsync(request, token), "spider");

            Assert.Contains("pre-active", log.Events);
            Assert.Contains("handler-active", log.Events);
            Assert.Contains("post-active", log.Events);
        }

        /// <summary>
        /// Verifies that successful pipelines complete the boundary.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenPipelineSucceeds_ShouldCompleteBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:complete" }, log.Events);
        }

        /// <summary>
        /// Verifies that preprocess failures fault the boundary.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenPreProcessThrows_ShouldFaultBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder =>
                {
                    builder.PreProcess<string, int>((ctx, args) => throw new InvalidOperationException("Pre failed."));
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException" }, log.Events);
        }

        /// <summary>
        /// Verifies that handler failures fault the boundary.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenHandlerThrows_ShouldFaultBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.ThrowAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException" }, log.Events);
        }

        /// <summary>
        /// Verifies that postprocess failures fault the boundary.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenPostProcessThrows_ShouldFaultBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder =>
                {
                    builder.OnSuccess<string, int>((ctx, args) => throw new InvalidOperationException("Post failed."));
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException" }, log.Events);
        }

        /// <summary>
        /// Verifies that cooperative cancellation cancels the boundary.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenPipelineCancels_ShouldCancelBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder =>
                {
                    builder.PreProcess<string, int>((ctx, args) =>
                    {
                        ctx.CancelOperation();
                        return Task.CompletedTask;
                    });
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:cancel" }, log.Events);
        }

        /// <summary>
        /// Verifies that multiple boundaries terminate in reverse registration order.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenMultipleBoundariesAreRegistered_ShouldNestInReverseOrder()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider()
                    .AddExecutionBoundary<FirstRecordingBoundary>()
                    .AddExecutionBoundary<SecondRecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "first:begin", "second:begin", "second:complete", "first:complete" }, log.Events);
        }

        /// <summary>
        /// Verifies that bridge-configured DI boundary types apply to the configured bridge.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsConfiguredByBridgeType_ShouldResolveBoundaryFromDi()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddScoped<SecondRecordingBoundary>();
                services.AddSpider();
            });

            await bridge
                .AddExecutionBoundary<SecondRecordingBoundary>()
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "second:begin", "second:complete" }, log.Events);
        }

        /// <summary>
        /// Verifies that bridge-configured boundary types can be selected by runtime type.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsConfiguredByBridgeTypeObject_ShouldResolveBoundaryFromDi()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddScoped<RecordingBoundary>();
                services.AddSpider();
            });

            await bridge
                .AddExecutionBoundary(typeof(RecordingBoundary))
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:complete" }, log.Events);
        }

        /// <summary>
        /// Verifies that bridge-selected boundaries apply to executions created from the bridge.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsConfiguredOnBridge_ShouldUseBridgeBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddScoped<RecordingBoundary>();
                services.AddSpider();
            });

            await bridge
                .AddExecutionBoundary<RecordingBoundary>()
                .Attach<string, int>(builder => { })
                .ExecuteAsync(
                    service => (request, token) => service.HandleAsync(request, token),
                    "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:complete" }, log.Events);
        }

        /// <summary>
        /// Verifies that bridge delegate boundary callbacks are executed for the configured bridge.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsConfiguredWithDelegates_ShouldRunDelegateBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider();
            });

            await bridge
                .AddExecutionBoundary(boundary =>
                {
                    boundary
                        .OnBegin((ctx, token) =>
                        {
                            log.Add($"delegate:begin:{ctx.RequestType.Name}");
                            return ValueTask.CompletedTask;
                        })
                        .OnComplete((ctx, token) =>
                        {
                            log.Add($"delegate:complete:{ctx.ResponseType?.Name}");
                            return ValueTask.CompletedTask;
                        })
                        .OnFault((ctx, ex, token) =>
                        {
                            log.Add($"delegate:fault:{ex.GetType().Name}");
                            return ValueTask.CompletedTask;
                        })
                        .OnCancel((ctx, token) =>
                        {
                            log.Add("delegate:cancel");
                            return ValueTask.CompletedTask;
                        });
                })
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "delegate:begin:String", "delegate:complete:Int32" }, log.Events);
        }

        /// <summary>
        /// Verifies that global and bridge-selected boundaries compose in deterministic order.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundariesComeFromAllLevels_ShouldComposeInOrder()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddScoped<SecondRecordingBoundary>();
                services.AddScoped<RecordingBoundary>();
                services.AddSpider().AddExecutionBoundary<FirstRecordingBoundary>();
            });

            await bridge
                .AddExecutionBoundary<SecondRecordingBoundary>()
                .AddExecutionBoundary(typeof(RecordingBoundary))
                .Attach<string, int>(builder => { })
                .ExecuteAsync(
                    service => (request, token) => service.HandleAsync(request, token),
                    "spider");

            Assert.Equal(
                new[] { "first:begin", "second:begin", "boundary:begin", "boundary:complete", "second:complete", "first:complete" },
                log.Events);
        }

        /// <summary>
        /// Verifies that a begin failure faults only the boundaries that already began.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenSecondBoundaryBeginFails_ShouldFaultOpenedBoundaries()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider()
                    .AddExecutionBoundary<FirstRecordingBoundary>()
                    .AddExecutionBoundary<ThrowingBeginBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "first:begin", "throw-begin:begin", "first:fault:InvalidOperationException" }, log.Events);
        }

        /// <summary>
        /// Verifies that fault failures during begin cleanup preserve the original begin exception.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBeginCleanupFaultAlsoFails_ShouldSurfaceBeginException()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider()
                    .AddExecutionBoundary<ThrowingFaultBoundary>()
                    .AddExecutionBoundary<ThrowingBeginBoundary>();
            });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal("Begin failed.", exception.Message);
            Assert.Equal(new[] { "throw-fault:begin", "throw-begin:begin", "throw-fault:fault:InvalidOperationException" }, log.Events);
        }

        /// <summary>
        /// Verifies direct boundary runner argument validation.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task RunAsync_WhenArgumentsAreNull_ShouldThrow()
        {
            using var provider = new ServiceCollection().BuildServiceProvider();
            var runner = new PipelineExecutionBoundaryRunner(provider);
            var requestContext = new RequestBoundaryContext(provider);
            var responseContext = new ResponseBoundaryContext(provider);

            await Assert.ThrowsAsync<ArgumentNullException>(() => runner.RunAsync<string>(
                null,
                () => Task.CompletedTask,
                Array.Empty<IPipelineExecutionBoundary>(),
                CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>(() => runner.RunAsync(
                requestContext,
                null,
                Array.Empty<IPipelineExecutionBoundary>(),
                CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>(() => runner.RunAsync(
                requestContext,
                () => Task.CompletedTask,
                null,
                CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>(() => runner.RunAsync<string, int>(
                null,
                () => Task.FromResult(42),
                Array.Empty<IPipelineExecutionBoundary>(),
                CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>(() => runner.RunAsync(
                responseContext,
                null,
                Array.Empty<IPipelineExecutionBoundary>(),
                CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>(() => runner.RunAsync(
                responseContext,
                () => Task.FromResult(42),
                null,
                CancellationToken.None));
        }

        /// <summary>
        /// Verifies traced boundary cancellation stays observable while preserving cancellation behavior.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task RunAsync_WhenTracedBoundaryBeginCancels_ShouldEmitCancelledBoundaryEvent()
        {
            var services = new ServiceCollection();
            services.AddSpiderRuntimeTracing();
            using var provider = services.BuildServiceProvider();
            var runner = new PipelineExecutionBoundaryRunner(provider);

            await Assert.ThrowsAsync<OperationCanceledException>(() => runner.RunAsync(
                new RequestBoundaryContext(provider),
                () => Task.CompletedTask,
                new IPipelineExecutionBoundary[] { new CancellingBeginBoundary() },
                CancellationToken.None));

            var trace = await GetOnlyTraceAsync(provider);

            Assert.Contains(trace.Events, item => item.ComponentKind == "spider.boundary" && item.Status == SpiderTraceStatus.Cancelled);
        }

        /// <summary>
        /// Verifies traced boundary failures stay observable while preserving exception behavior.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task RunAsync_WhenTracedBoundaryBeginFaults_ShouldEmitFaultedBoundaryEvent()
        {
            var services = new ServiceCollection();
            services.AddSpiderRuntimeTracing();
            using var provider = services.BuildServiceProvider();
            var runner = new PipelineExecutionBoundaryRunner(provider);

            await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(
                new RequestBoundaryContext(provider),
                () => Task.CompletedTask,
                new IPipelineExecutionBoundary[] { new DirectThrowingBeginBoundary() },
                CancellationToken.None));

            var trace = await GetOnlyTraceAsync(provider);

            Assert.Contains(trace.Events, item => item.ComponentKind == "spider.boundary" && item.Status == SpiderTraceStatus.Faulted);
        }

        /// <summary>
        /// Verifies that complete failures surface when the pipeline succeeded.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenCompleteFails_ShouldSurfaceCompleteException()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider()
                    .AddExecutionBoundary<FirstRecordingBoundary>()
                    .AddExecutionBoundary<ThrowingCompleteBoundary>();
            });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal("Complete failed.", exception.Message);
            Assert.Equal(new[] { "first:begin", "throw-complete:begin", "throw-complete:complete" }, log.Events);
        }

        /// <summary>
        /// Verifies that fault failures do not replace the original pipeline exception.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenFaultFails_ShouldSurfaceOriginalPipelineException()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<ThrowingFaultBoundary>();
            });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.ThrowAsync(request, token), "spider"));

            Assert.Equal("Handler failed.", exception.Message);
            Assert.Equal(new[] { "throw-fault:begin", "throw-fault:fault:InvalidOperationException" }, log.Events);
        }

        /// <summary>
        /// Creates a service bridge for boundary tests.
        /// </summary>
        /// <param name="configure">An optional service configuration action.</param>
        /// <returns>A service bridge for the test service.</returns>
        private static IServiceBridge<BoundaryTestService> CreateBridge(Action<IServiceCollection> configure = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton<BoundaryTestService>();

            if (configure == null)
                services.AddSpider();
            else
                configure(services);

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<ISpider>().InitBridge<BoundaryTestService>();
        }

        private static async Task<SpiderTrace> GetOnlyTraceAsync(IServiceProvider provider)
        {
            await provider.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync(CancellationToken.None);
            var reader = provider.GetRequiredService<ISpiderTraceReader>();
            var summaries = new List<SpiderTraceSummary>();
            await foreach (var summary in reader.QueryAsync(new SpiderTraceQuery { Limit = 10 }, CancellationToken.None))
                summaries.Add(summary);

            var only = Assert.Single(summaries);
            return await reader.GetAsync(only.TraceId, CancellationToken.None);
        }

        private sealed class RequestBoundaryContext : IReadOnlyContext<string>
        {
            public RequestBoundaryContext(IServiceProvider services, bool cancelled = false)
            {
                Services = services;
                Cancelled = cancelled;
            }

            public string Request => "spider";

            public IServiceProvider Services { get; }

            public bool Cancelled { get; }

            public PipelineState PipelineState => PipelineState.OnTargeting;

            public CancellationToken CancellationToken => CancellationToken.None;

            public ResultState ResultState => ResultState.Pending;

            public Exception Exception => null;
        }

        private sealed class ResponseBoundaryContext : IReadOnlyContext<string, int>
        {
            public ResponseBoundaryContext(IServiceProvider services, bool cancelled = false)
            {
                Services = services;
                Cancelled = cancelled;
            }

            public string Request => "spider";

            public int Response => 6;

            public IServiceProvider Services { get; }

            public bool Cancelled { get; }

            public PipelineState PipelineState => PipelineState.OnTargeting;

            public CancellationToken CancellationToken => CancellationToken.None;

            public ResultState ResultState => ResultState.Pending;

            public Exception Exception => null;
        }

        private sealed class CancellingBeginBoundary : PipelineExecutionBoundary
        {
            public override ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
                => throw new OperationCanceledException("Boundary cancelled.");
        }

        private sealed class DirectThrowingBeginBoundary : PipelineExecutionBoundary
        {
            public override ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
                => throw new InvalidOperationException("Direct begin failed.");
        }
    }
}
