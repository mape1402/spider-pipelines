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

        [Fact]
        public async Task OnTargetExecution_WhenContextIsCancelled_ShouldSkipTargetAndOverride()
        {
            var execution = new TargetExecution<string>(
                (req, token) => throw new InvalidOperationException("Override should not run."),
                (ctx, args) => true);

            await execution.OnTargetExecution(
                new ReadOnlyContextStub(cancelled: true),
                (req, token) => throw new InvalidOperationException("Target should not run."));
        }

        [Theory]
        [InlineData(true, "override")]
        [InlineData(false, "target")]
        public async Task OnTargetExecution_WhenOverrideConditionIsConfigured_ShouldSelectExpectedHandler(bool overrideCondition, string expected)
        {
            var calls = new List<string>();
            var execution = new TargetExecution<string>(
                (req, token) =>
                {
                    calls.Add("override");
                    return Task.CompletedTask;
                },
                (ctx, args) => overrideCondition);

            await execution.OnTargetExecution(
                new ReadOnlyContextStub(),
                (req, token) =>
                {
                    calls.Add("target");
                    return Task.CompletedTask;
                });

            Assert.Equal(new[] { expected }, calls);
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

        [Fact]
        public async Task OnTargetExecution_WhenContextIsCancelled_ShouldReturnDefaultAndSkipHandlers()
        {
            var execution = new TargetExecution<string, int>(
                (req, token) => throw new InvalidOperationException("Override should not run."),
                (ctx, args) => true);

            var result = await execution.OnTargetExecution(
                new ReadOnlyContextGenericStub(cancelled: true),
                (req, token) => throw new InvalidOperationException("Target should not run."));

            Assert.Equal(default, result);
        }

        [Theory]
        [InlineData(true, 7)]
        [InlineData(false, 42)]
        public async Task OnTargetExecution_WhenOverrideConditionIsConfigured_ShouldSelectExpectedHandler(bool overrideCondition, int expected)
        {
            var execution = new TargetExecution<string, int>(
                (req, token) => Task.FromResult(7),
                (ctx, args) => overrideCondition);

            var result = await execution.OnTargetExecution(
                new ReadOnlyContextGenericStub(),
                (req, token) => Task.FromResult(42));

            Assert.Equal(expected, result);
        }
    }

    // Stub for IReadOnlyContext<string>
    public class ReadOnlyContextStub : IReadOnlyContext<string>
    {
        private readonly bool _cancelled;

        public ReadOnlyContextStub(bool cancelled = false)
            => _cancelled = cancelled;

        public string Request => "test";
        public IServiceProvider Services => null;
        public bool Cancelled => _cancelled;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }

    // Stub for IReadOnlyContext<string, int>
    public class ReadOnlyContextGenericStub : IReadOnlyContext<string, int>
    {
        private readonly bool _cancelled;

        public ReadOnlyContextGenericStub(bool cancelled = false)
            => _cancelled = cancelled;

        public string Request => "test";
        public int Response => 42;
        public IServiceProvider Services => null;
        public bool Cancelled => _cancelled;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }
}
