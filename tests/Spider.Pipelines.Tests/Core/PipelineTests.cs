using Spider.Pipelines.Core;
using Spider.Pipelines.Core.Internals;
using Spider.Pipelines.Extensions;
using Spider.Pipelines.RuntimeTracing;
using Spider.Pipelines.Targeting;
using Spider.Pipelines.Tests.Boundaries;
using Microsoft.Extensions.DependencyInjection;

namespace Spider.Pipelines.Tests.Core
{
    public class PipelineTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var plan = new ExecutionPlanStub();
            var pipeline = new Pipeline<string>((req, token) => Task.CompletedTask, plan, new ServiceProviderStub());
            Assert.NotNull(pipeline);
        }

        [Fact]
        public void Constructor_WhenArgumentsAreMissing_ShouldThrow()
        {
            var plan = new ExecutionPlanStub();
            var provider = new ServiceProviderStub();

            Assert.Throws<ArgumentNullException>(() => new Pipeline<string>(null, plan, provider));
            Assert.Throws<ArgumentNullException>(() => new Pipeline<string>((req, token) => Task.CompletedTask, null, provider));
            Assert.Throws<ArgumentNullException>(() => new Pipeline<string>((req, token) => Task.CompletedTask, plan, null));
        }

        [Fact]
        public async Task RunAsync_WhenPipelineSucceedsWithExecutionBoundary_ShouldRunAllStages()
        {
            var log = new BoundaryEventLog();
            using var provider = PipelineTestServices.CreateProvider(log);
            var plan = new RecordingExecutionPlan();
            var targetRan = false;
            var pipeline = new Pipeline<string>(
                (req, token) =>
                {
                    targetRan = true;
                    return Task.CompletedTask;
                },
                plan,
                provider);

            await pipeline.RunAsync("request", execution => execution.AddExecutionBoundary<RecordingBoundary>());

            Assert.True(targetRan);
            Assert.Equal(new[] { "pre", "target", "post" }, plan.Events);
            Assert.Equal(new[] { "boundary:begin", "boundary:complete" }, log.Events);
        }

        [Fact]
        public async Task RunAsync_WhenContextIsMarkedFailure_ShouldThrowContextException()
        {
            var plan = new FailingResultExecutionPlan();
            var pipeline = new Pipeline<string>((req, token) => Task.CompletedTask, plan, new ServiceProviderStub());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));

            Assert.Equal("Context failed.", exception.Message);
        }

        [Fact]
        public async Task RunAsync_WhenExecutionConfigurationIsMissing_ShouldThrow()
        {
            var plan = new ExecutionPlanStub();
            var pipeline = new Pipeline<string>((req, token) => Task.CompletedTask, plan, new ServiceProviderStub());

            await Assert.ThrowsAsync<ArgumentNullException>(() => pipeline.RunAsync("request", null));
        }

        [Fact]
        public async Task RunAsync_WhenPreProcessFails_ShouldNotRunTargetingOrPostProcess()
        {
            var plan = new ThrowingPreProcessExecutionPlanStub();
            var pipeline = new Pipeline<string>((req, token) => Task.CompletedTask, plan, new ServiceProviderStub());

            await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));

            Assert.False(plan.TargetingRan);
            Assert.False(plan.PostProcessRan);
        }

        [Fact]
        public async Task RunAsync_WhenRuntimeTracingIsEnabled_ShouldEmitRequestOnlyPipelineTrace()
        {
            using var provider = PipelineTestServices.CreateTracingProvider();
            var plan = new RecordingExecutionPlan();
            var pipeline = new Pipeline<string>((req, token) => Task.CompletedTask, plan, provider);

            await pipeline.RunAsync("request");

            var trace = await PipelineTestServices.GetOnlyTraceAsync(provider);

            Assert.Equal(SpiderTraceStatus.Completed, trace.Status);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStarted && item.OutputType == null);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineCompleted);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStageStarted && item.DisplayName == "Pre-process");
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStageCompleted && item.DisplayName == "Post-process");
        }

        [Fact]
        public async Task RunAsync_WhenRuntimeTracedRequestOnlyPipelineFaults_ShouldEmitFaultedTrace()
        {
            using var provider = PipelineTestServices.CreateTracingProvider();
            var plan = new ThrowingTargetExecutionPlan();
            var pipeline = new Pipeline<string>(
                (req, token) => throw new InvalidOperationException("Target failed."),
                plan,
                provider);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));
            var trace = await PipelineTestServices.GetOnlyTraceAsync(provider);

            Assert.Equal("Target failed.", exception.Message);
            Assert.Equal(SpiderTraceStatus.Faulted, trace.Status);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineFaulted);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStageFaulted && item.DisplayName == "Target");
        }

        [Fact]
        public async Task RunAsync_WhenRuntimeTracedRequestOnlyPipelineIsCancelled_ShouldEmitCancelledTrace()
        {
            using var provider = PipelineTestServices.CreateTracingProvider();
            var plan = new ThrowingTargetExecutionPlan();
            var pipeline = new Pipeline<string>(
                (req, token) => throw new OperationCanceledException("Target cancelled."),
                plan,
                provider);

            await Assert.ThrowsAsync<OperationCanceledException>(() => pipeline.RunAsync("request"));
            var trace = await PipelineTestServices.GetOnlyTraceAsync(provider);

            Assert.Equal(SpiderTraceStatus.Cancelled, trace.Status);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineCancelled);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStageFaulted && item.Status == SpiderTraceStatus.Cancelled);
        }
    }

    public class PipelineGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var plan = new ExecutionPlanGenericStub();
            var pipeline = new Pipeline<string, int>((req, token) => Task.FromResult(42), plan, new ServiceProviderStub());
            Assert.NotNull(pipeline);
        }

        [Fact]
        public void Constructor_WhenArgumentsAreMissing_ShouldThrow()
        {
            var plan = new ExecutionPlanGenericStub();
            var provider = new ServiceProviderStub();

            Assert.Throws<ArgumentNullException>(() => new Pipeline<string, int>(null, plan, provider));
            Assert.Throws<ArgumentNullException>(() => new Pipeline<string, int>((req, token) => Task.FromResult(42), null, provider));
            Assert.Throws<ArgumentNullException>(() => new Pipeline<string, int>((req, token) => Task.FromResult(42), plan, null));
        }

        [Fact]
        public async Task RunAsync_WhenPipelineSucceedsWithExecutionBoundary_ShouldReturnResponse()
        {
            var log = new BoundaryEventLog();
            using var provider = PipelineTestServices.CreateProvider(log);
            var plan = new RecordingExecutionPlanGeneric();
            var pipeline = new Pipeline<string, int>((req, token) => Task.FromResult(req.Length), plan, provider);

            var response = await pipeline.RunAsync("request", execution => execution.AddExecutionBoundary<RecordingBoundary>());

            Assert.Equal(7, response);
            Assert.Equal(new[] { "pre", "target", "post" }, plan.Events);
            Assert.Equal(new[] { "boundary:begin", "boundary:complete" }, log.Events);
        }

        [Fact]
        public async Task RunAsync_WhenContextIsMarkedFailure_ShouldThrowContextException()
        {
            var plan = new FailingResultExecutionPlanGeneric();
            var pipeline = new Pipeline<string, int>((req, token) => Task.FromResult(42), plan, new ServiceProviderStub());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));

            Assert.Equal("Context failed.", exception.Message);
        }

        [Fact]
        public async Task RunAsync_WhenExecutionConfigurationIsMissing_ShouldThrow()
        {
            var plan = new ExecutionPlanGenericStub();
            var pipeline = new Pipeline<string, int>((req, token) => Task.FromResult(42), plan, new ServiceProviderStub());

            await Assert.ThrowsAsync<ArgumentNullException>(() => pipeline.RunAsync("request", null));
        }

        [Fact]
        public async Task RunAsync_WhenPreProcessFails_ShouldNotRunTargetingOrPostProcess()
        {
            var plan = new ThrowingPreProcessExecutionPlanGenericStub();
            var pipeline = new Pipeline<string, int>((req, token) => Task.FromResult(42), plan, new ServiceProviderStub());

            await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));

            Assert.False(plan.TargetingRan);
            Assert.False(plan.PostProcessRan);
        }

        [Fact]
        public async Task RunAsync_WhenRuntimeTracedGenericPipelineFaults_ShouldEmitFaultedTrace()
        {
            using var provider = PipelineTestServices.CreateTracingProvider();
            var plan = new ThrowingTargetExecutionPlanGeneric();
            var pipeline = new Pipeline<string, int>(
                (req, token) => throw new InvalidOperationException("Generic target failed."),
                plan,
                provider);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));
            var trace = await PipelineTestServices.GetOnlyTraceAsync(provider);

            Assert.Equal("Generic target failed.", exception.Message);
            Assert.Equal(SpiderTraceStatus.Faulted, trace.Status);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineFaulted);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStageFaulted && item.DisplayName == "Target");
        }

        [Fact]
        public async Task RunAsync_WhenRuntimeTracedGenericPipelineIsCancelled_ShouldEmitCancelledTrace()
        {
            using var provider = PipelineTestServices.CreateTracingProvider();
            var plan = new ThrowingTargetExecutionPlanGeneric();
            var pipeline = new Pipeline<string, int>(
                (req, token) => throw new OperationCanceledException("Generic target cancelled."),
                plan,
                provider);

            await Assert.ThrowsAsync<OperationCanceledException>(() => pipeline.RunAsync("request"));
            var trace = await PipelineTestServices.GetOnlyTraceAsync(provider);

            Assert.Equal(SpiderTraceStatus.Cancelled, trace.Status);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineCancelled);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStageFaulted && item.Status == SpiderTraceStatus.Cancelled);
        }
    }

    internal static class PipelineTestServices
    {
        public static ServiceProvider CreateProvider(BoundaryEventLog log)
        {
            var services = new ServiceCollection();
            services.AddSingleton(log);
            services.AddTransient<RecordingBoundary>();
            return services.BuildServiceProvider();
        }

        public static ServiceProvider CreateTracingProvider()
        {
            var services = new ServiceCollection();
            services.AddSpiderRuntimeTracing();
            return services.BuildServiceProvider();
        }

        public static async Task<SpiderTrace> GetOnlyTraceAsync(IServiceProvider provider)
        {
            var dispatcher = provider.GetRequiredService<ISpiderTraceDispatcher>();
            await dispatcher.FlushAsync(CancellationToken.None);

            var reader = provider.GetRequiredService<ISpiderTraceReader>();
            var summaries = new List<SpiderTraceSummary>();
            await foreach (var summary in reader.QueryAsync(new SpiderTraceQuery { Limit = 10 }, CancellationToken.None))
                summaries.Add(summary);

            var only = Assert.Single(summaries);
            return await reader.GetAsync(only.TraceId, CancellationToken.None);
        }
    }

    // Stubs for dependencies
    public class ExecutionPlanStub : IExecutionPlan<string>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
        public Task OnTargetingAsync(IReadOnlyContext<string> context, TargetHandler<string> handler) => Task.CompletedTask;
        public Task OnPostProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
    }

    public class RecordingExecutionPlan : IExecutionPlan<string>
    {
        public IList<string> Events { get; } = new List<string>();

        public Task OnPreProcessAsync(IReadOnlyContext<string> context)
        {
            Events.Add("pre");
            Assert.Equal(PipelineState.OnPreProcess, context.PipelineState);
            return Task.CompletedTask;
        }

        public async Task OnTargetingAsync(IReadOnlyContext<string> context, TargetHandler<string> handler)
        {
            Events.Add("target");
            Assert.Equal(PipelineState.OnTargeting, context.PipelineState);
            await handler(context.Request, context.CancellationToken);
        }

        public Task OnPostProcessAsync(IReadOnlyContext<string> context)
        {
            Events.Add("post");
            Assert.Equal(PipelineState.OnPostProcess, context.PipelineState);
            context.AsSettable().Success();
            return Task.CompletedTask;
        }
    }

    public class FailingResultExecutionPlan : IExecutionPlan<string>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;

        public Task OnTargetingAsync(IReadOnlyContext<string> context, TargetHandler<string> handler)
            => handler(context.Request, context.CancellationToken);

        public Task OnPostProcessAsync(IReadOnlyContext<string> context)
        {
            context.AsSettable().Failure(new InvalidOperationException("Context failed."));
            return Task.CompletedTask;
        }
    }

    public class ExecutionPlanGenericStub : IExecutionPlan<string, int>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
        public Task<int> OnTargetingAsync(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler) => Task.FromResult(42);
        public Task OnPostProcessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
    }

    public class RecordingExecutionPlanGeneric : IExecutionPlan<string, int>
    {
        public IList<string> Events { get; } = new List<string>();

        public Task OnPreProcessAsync(IReadOnlyContext<string, int> context)
        {
            Events.Add("pre");
            Assert.Equal(PipelineState.OnPreProcess, context.PipelineState);
            return Task.CompletedTask;
        }

        public async Task<int> OnTargetingAsync(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler)
        {
            Events.Add("target");
            Assert.Equal(PipelineState.OnTargeting, context.PipelineState);
            return await handler(context.Request, context.CancellationToken);
        }

        public Task OnPostProcessAsync(IReadOnlyContext<string, int> context)
        {
            Events.Add("post");
            Assert.Equal(PipelineState.OnPostProcess, context.PipelineState);
            context.AsSettable().Success(7);
            return Task.CompletedTask;
        }
    }

    public class FailingResultExecutionPlanGeneric : IExecutionPlan<string, int>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;

        public Task<int> OnTargetingAsync(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler)
            => handler(context.Request, context.CancellationToken);

        public Task OnPostProcessAsync(IReadOnlyContext<string, int> context)
        {
            context.AsSettable().Failure(new InvalidOperationException("Context failed."));
            return Task.CompletedTask;
        }
    }

    public class ThrowingPreProcessExecutionPlanStub : IExecutionPlan<string>
    {
        public bool TargetingRan { get; private set; }
        public bool PostProcessRan { get; private set; }

        public Task OnPreProcessAsync(IReadOnlyContext<string> context)
            => throw new InvalidOperationException("Preprocess failed.");

        public Task OnTargetingAsync(IReadOnlyContext<string> context, TargetHandler<string> handler)
        {
            TargetingRan = true;
            return Task.CompletedTask;
        }

        public Task OnPostProcessAsync(IReadOnlyContext<string> context)
        {
            PostProcessRan = true;
            return Task.CompletedTask;
        }
    }

    public class ThrowingTargetExecutionPlan : IExecutionPlan<string>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;

        public Task OnTargetingAsync(IReadOnlyContext<string> context, TargetHandler<string> handler)
            => handler(context.Request, context.CancellationToken);

        public Task OnPostProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
    }

    public class ThrowingPreProcessExecutionPlanGenericStub : IExecutionPlan<string, int>
    {
        public bool TargetingRan { get; private set; }
        public bool PostProcessRan { get; private set; }

        public Task OnPreProcessAsync(IReadOnlyContext<string, int> context)
            => throw new InvalidOperationException("Preprocess failed.");

        public Task<int> OnTargetingAsync(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler)
        {
            TargetingRan = true;
            return Task.FromResult(0);
        }

        public Task OnPostProcessAsync(IReadOnlyContext<string, int> context)
        {
            PostProcessRan = true;
            return Task.CompletedTask;
        }
    }

    public class ThrowingTargetExecutionPlanGeneric : IExecutionPlan<string, int>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;

        public Task<int> OnTargetingAsync(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler)
            => handler(context.Request, context.CancellationToken);

        public Task OnPostProcessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
    }
}
