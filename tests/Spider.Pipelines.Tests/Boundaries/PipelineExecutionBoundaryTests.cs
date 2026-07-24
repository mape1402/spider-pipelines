using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;

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
                services.AddSpider().AddExecutionBoundary<string, int, RecordingBoundary>();
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
                services.AddSpider().AddExecutionBoundary<string, int, RecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:complete", "boundary:dispose" }, log.Events);
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
                services.AddSpider().AddExecutionBoundary<string, int, RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder =>
                {
                    builder.PreProcess<string, int>((ctx, args) => throw new InvalidOperationException("Pre failed."));
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException", "boundary:dispose" }, log.Events);
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
                services.AddSpider().AddExecutionBoundary<string, int, RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.ThrowAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException", "boundary:dispose" }, log.Events);
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
                services.AddSpider().AddExecutionBoundary<string, int, RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder =>
                {
                    builder.OnSuccess<string, int>((ctx, args) => throw new InvalidOperationException("Post failed."));
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException", "boundary:dispose" }, log.Events);
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
                services.AddSpider().AddExecutionBoundary<string, int, RecordingBoundary>();
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

            Assert.Equal(new[] { "boundary:begin", "boundary:cancel", "boundary:dispose" }, log.Events);
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
                    .AddExecutionBoundary<string, int, FirstRecordingBoundary>()
                    .AddExecutionBoundary<string, int, SecondRecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "first:begin", "second:begin", "second:complete", "first:complete", "second:dispose", "first:dispose" }, log.Events);
        }

        /// <summary>
        /// Verifies that fluent-configured DI boundary types apply only to the configured pipeline.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsConfiguredByFluentType_ShouldResolveBoundaryFromDi()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddScoped<SecondRecordingBoundary>();
                services.AddSpider();
            });

            await bridge
                .Attach<string, int>(builder => builder.AddExecutionBoundary<SecondRecordingBoundary>())
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "second:begin", "second:complete", "second:dispose" }, log.Events);
        }

        /// <summary>
        /// Verifies that fluent-configured boundary instances apply only to the configured pipeline.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsConfiguredByFluentInstance_ShouldUseBoundaryInstance()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider();
            });

            await bridge
                .Attach<string, int>(builder => builder.AddExecutionBoundary(new RecordingBoundary(log)))
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:complete", "boundary:dispose" }, log.Events);
        }

        /// <summary>
        /// Verifies that fluent-configured delegate boundaries can use typed request and response data.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsConfiguredByDelegates_ShouldUseTypedCallbacks()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider();
            });

            await bridge
                .Attach<string, int>(builder =>
                {
                    builder.AddExecutionBoundary(boundary =>
                    {
                        boundary.OnBegin((ctx, token) =>
                        {
                            log.Add($"delegate:begin:{ctx.Request}");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnComplete((ctx, token) =>
                        {
                            log.Add($"delegate:complete:{ctx.Response}");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnFault((ctx, ex, token) =>
                        {
                            log.Add($"delegate:fault:{ex.GetType().Name}");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnCancel((ctx, token) =>
                        {
                            log.Add("delegate:cancel");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnDispose(ctx =>
                        {
                            log.Add("delegate:dispose");
                            return ValueTask.CompletedTask;
                        });
                    });
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "delegate:begin:spider", "delegate:complete:6", "delegate:dispose" }, log.Events);
        }

        /// <summary>
        /// Verifies that invocation-specific boundaries apply only to the current execution.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenBoundaryIsProvidedAtInvocation_ShouldUseInvocationBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider();
            });

            await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(
                    service => (request, token) => service.HandleAsync(request, token),
                    "spider",
                    new[] { new RecordingBoundary(log) });

            Assert.Equal(new[] { "boundary:begin", "boundary:complete", "boundary:dispose" }, log.Events);
        }

        /// <summary>
        /// Verifies that global, fluent, and invocation boundaries compose in deterministic order.
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
                services.AddSpider().AddExecutionBoundary<string, int, FirstRecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder => builder.AddExecutionBoundary<SecondRecordingBoundary>())
                .ExecuteAsync(
                    service => (request, token) => service.HandleAsync(request, token),
                    "spider",
                    new[] { new RecordingBoundary(log) });

            Assert.Equal(
                new[] { "first:begin", "second:begin", "boundary:begin", "boundary:complete", "second:complete", "first:complete", "boundary:dispose", "second:dispose", "first:dispose" },
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
                    .AddExecutionBoundary<string, int, FirstRecordingBoundary>()
                    .AddExecutionBoundary<string, int, ThrowingBeginBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "first:begin", "throw-begin:begin", "first:fault:InvalidOperationException", "first:dispose" }, log.Events);
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
                    .AddExecutionBoundary<string, int, FirstRecordingBoundary>()
                    .AddExecutionBoundary<string, int, ThrowingCompleteBoundary>();
            });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal("Complete failed.", exception.Message);
            Assert.Equal(new[] { "first:begin", "throw-complete:begin", "throw-complete:complete", "throw-complete:dispose", "first:dispose" }, log.Events);
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
                services.AddSpider().AddExecutionBoundary<string, int, ThrowingFaultBoundary>();
            });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.ThrowAsync(request, token), "spider"));

            Assert.Equal("Handler failed.", exception.Message);
            Assert.Equal(new[] { "throw-fault:begin", "throw-fault:fault:InvalidOperationException", "throw-fault:dispose" }, log.Events);
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
    }
}
