using Spider.Pipelines.Core;
using Spider.Pipelines.Core.Internals;
using Spider.Pipelines.Targeting;

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
        public async Task RunAsync_WhenPreProcessFails_ShouldNotRunTargetingOrPostProcess()
        {
            var plan = new ThrowingPreProcessExecutionPlanStub();
            var pipeline = new Pipeline<string>((req, token) => Task.CompletedTask, plan, new ServiceProviderStub());

            await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));

            Assert.False(plan.TargetingRan);
            Assert.False(plan.PostProcessRan);
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
        public async Task RunAsync_WhenPreProcessFails_ShouldNotRunTargetingOrPostProcess()
        {
            var plan = new ThrowingPreProcessExecutionPlanGenericStub();
            var pipeline = new Pipeline<string, int>((req, token) => Task.FromResult(42), plan, new ServiceProviderStub());

            await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.RunAsync("request"));

            Assert.False(plan.TargetingRan);
            Assert.False(plan.PostProcessRan);
        }
    }

    // Stubs for dependencies
    public class ExecutionPlanStub : IExecutionPlan<string>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
        public Task OnTargetingAsync(IReadOnlyContext<string> context, TargetHandler<string> handler) => Task.CompletedTask;
        public Task OnPostProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
    }
    public class ExecutionPlanGenericStub : IExecutionPlan<string, int>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
        public Task<int> OnTargetingAsync(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler) => Task.FromResult(42);
        public Task OnPostProcessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
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
}
