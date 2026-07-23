using Spider.Pipelines.Core;
using Spider.Pipelines.Targeting;

namespace Spider.Pipelines.Tests.Targeting
{
    public class TargetExecutionTests
    {
        [Fact]
        public async Task OnTargetExecution_ShouldInvokeTargetHandler_WhenNoOverride()
        {
            var called = false;
            var execution = new TargetExecution<string>(null, null);
            await execution.OnTargetExecution(new ReadOnlyContextStub(), (req, token) => { called = true; return Task.CompletedTask; });
            Assert.True(called);
        }

        [Fact]
        public async Task OnTargetExecution_ShouldInvokeOverride_WhenOverrideHasNoCondition()
        {
            var targetCalled = false;
            var overrideCalled = false;
            var execution = new TargetExecution<string>((req, token) =>
            {
                overrideCalled = true;
                return Task.CompletedTask;
            }, null);

            await execution.OnTargetExecution(new ReadOnlyContextStub(), (req, token) =>
            {
                targetCalled = true;
                return Task.CompletedTask;
            });

            Assert.True(overrideCalled);
            Assert.False(targetCalled);
        }
    }

    public class TargetExecutionGenericTests
    {
        [Fact]
        public async Task OnTargetExecution_ShouldInvokeTargetHandler_WhenNoOverride()
        {
            var called = false;
            var execution = new TargetExecution<string, int>(null, null);
            await execution.OnTargetExecution(new ReadOnlyContextGenericStub(), (req, token) => { called = true; return Task.FromResult(42); });
            Assert.True(called);
        }

        [Fact]
        public async Task OnTargetExecution_ShouldInvokeOverride_WhenOverrideHasNoCondition()
        {
            var targetCalled = false;
            var overrideCalled = false;
            var execution = new TargetExecution<string, int>((req, token) =>
            {
                overrideCalled = true;
                return Task.FromResult(7);
            }, null);

            var result = await execution.OnTargetExecution(new ReadOnlyContextGenericStub(), (req, token) =>
            {
                targetCalled = true;
                return Task.FromResult(42);
            });

            Assert.Equal(7, result);
            Assert.True(overrideCalled);
            Assert.False(targetCalled);
        }
    }

    // Stub for IReadOnlyContext<string>
    public class ReadOnlyContextStub : IReadOnlyContext<string>
    {
        public string Request => "test";
        public IServiceProvider Services => null;
        public bool Cancelled => false;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }

    // Stub for IReadOnlyContext<string, int>
    public class ReadOnlyContextGenericStub : IReadOnlyContext<string, int>
    {
        public string Request => "test";
        public int Response => 42;
        public IServiceProvider Services => null;
        public bool Cancelled => false;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }
}
