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
}
