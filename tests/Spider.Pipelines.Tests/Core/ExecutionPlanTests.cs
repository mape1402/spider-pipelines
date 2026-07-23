using System;
using Xunit;
using Spider.Pipelines.Core.Internals;
using Spider.Pipelines.Core;
using Spider.Pipelines.Parallelization;
using Spider.Pipelines.PostProcessing;
using Spider.Pipelines.PreProcessing;
using Spider.Pipelines.Targeting;

namespace Spider.Pipelines.Tests.Core
{
    public class ExecutionPlanTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var pre = new PreProcessExecutionStub();
            var target = new TargetExecutionStub();
            var parallel = new ParallelExecutionStub();
            var post = new PostProcessExecutionStub();
            var plan = new ExecutionPlan<string>(pre, target, parallel, post);
            Assert.NotNull(plan);
        }

        [Fact]
        public async Task OnTargetingAsync_WhenContextIsCancelled_ShouldMarkCancelledAndSkipTarget()
        {
            var context = new Context<string>("request", new ServiceProviderStub());
            context.CancelOperation();
            var target = new RecordingTargetExecutionStub();
            var plan = new ExecutionPlan<string>(
                new PreProcessExecutionStub(),
                target,
                new ParallelExecutionStub(),
                new PostProcessExecutionStub());

            await plan.OnTargetingAsync(context, (req, token) => Task.CompletedTask);

            Assert.False(target.Called);
            Assert.Equal(ResultState.Cancelled, context.ResultState);
        }
    }

    public class ExecutionPlanGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var pre = new PreProcessExecutionStub();
            var target = new TargetExecutionGenericStub();
            var parallel = new ParallelExecutionGenericStub();
            var post = new PostProcessExecutionGenericStub();
            var plan = new ExecutionPlan<string, int>(pre, target, parallel, post);
            Assert.NotNull(plan);
        }

        [Fact]
        public async Task OnTargetingAsync_WhenContextIsCancelled_ShouldMarkCancelledAndSkipTarget()
        {
            var context = new Context<string, int>("request", new ServiceProviderStub());
            context.CancelOperation();
            var target = new RecordingTargetExecutionGenericStub();
            var plan = new ExecutionPlan<string, int>(
                new PreProcessExecutionStub(),
                target,
                new ParallelExecutionGenericStub(),
                new PostProcessExecutionGenericStub());

            var response = await plan.OnTargetingAsync(context, (req, token) => Task.FromResult(42));

            Assert.False(target.Called);
            Assert.Equal(default, response);
            Assert.Equal(ResultState.Cancelled, context.ResultState);
        }
    }

    // Stubs for dependencies
    public class PreProcessExecutionStub : IPreProcessExecution<string>
    {
        public Task OnPreProcessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
    }
    public class TargetExecutionStub : ITargetExecution<string>
    {
        public Task OnTargetExecution(IReadOnlyContext<string> context, TargetHandler<string> handler) => Task.CompletedTask;
    }
    public class RecordingTargetExecutionStub : ITargetExecution<string>
    {
        public bool Called { get; private set; }

        public Task OnTargetExecution(IReadOnlyContext<string> context, TargetHandler<string> handler)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }
    public class ParallelExecutionStub : IParallelExecution<string>
    {
        public Task OnParallelAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
    }
    public class PostProcessExecutionStub : IPostProcessExecution<string>
    {
        public Task OnSuccessAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
        public Task OnFailureAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
    }
    public class TargetExecutionGenericStub : ITargetExecution<string, int>
    {
        public Task<int> OnTargetExecution(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler) => Task.FromResult(42);
    }
    public class RecordingTargetExecutionGenericStub : ITargetExecution<string, int>
    {
        public bool Called { get; private set; }

        public Task<int> OnTargetExecution(IReadOnlyContext<string, int> context, TargetHandler<string, int> handler)
        {
            Called = true;
            return Task.FromResult(42);
        }
    }
    public class ParallelExecutionGenericStub : IParallelExecution<string, int>
    {
        public Task OnParallelAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
    }
    public class PostProcessExecutionGenericStub : IPostProcessExecution<string, int>
    {
        public Task OnSuccessAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
        public Task OnFailureAsync(IReadOnlyContext<string, int> context) => Task.CompletedTask;
        // Implement missing interface member for OnFailureAsync(IReadOnlyContext<string>)
        public Task OnFailureAsync(IReadOnlyContext<string> context) => Task.CompletedTask;
    }
}
