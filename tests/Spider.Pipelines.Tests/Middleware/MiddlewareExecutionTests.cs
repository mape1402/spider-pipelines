using Spider.Pipelines.Core;
using Spider.Pipelines.Middleware;

namespace Spider.Pipelines.Tests.Middleware
{
    public class MiddlewareExecutionTests
    {
        [Fact]
        public async Task OnMiddlewareAsync_ShouldWrapTargetInRegistrationOrder()
        {
            var events = new List<string>();
            var execution = new MiddlewareExecution<string>(new PipelineMiddlewareDelegate<string>[]
            {
                async (ctx, next) =>
                {
                    events.Add("first-before");
                    await next();
                    events.Add("first-after");
                },
                async (ctx, next) =>
                {
                    events.Add("second-before");
                    await next();
                    events.Add("second-after");
                }
            });

            await execution.OnMiddlewareAsync(new ReadOnlyContextStub(), () =>
            {
                events.Add("target");
                return Task.CompletedTask;
            });

            Assert.Equal(new[] { "first-before", "second-before", "target", "second-after", "first-after" }, events);
        }

        [Fact]
        public async Task OnMiddlewareAsync_WhenMiddlewareSkipsNext_ShouldSkipTarget()
        {
            var targetCalled = false;
            var execution = new MiddlewareExecution<string>(new PipelineMiddlewareDelegate<string>[]
            {
                (ctx, next) => Task.CompletedTask
            });

            await execution.OnMiddlewareAsync(new ReadOnlyContextStub(), () =>
            {
                targetCalled = true;
                return Task.CompletedTask;
            });

            Assert.False(targetCalled);
        }
    }

    public class MiddlewareExecutionGenericTests
    {
        [Fact]
        public async Task OnMiddlewareAsync_ShouldAllowMiddlewareToTransformResponse()
        {
            var execution = new MiddlewareExecution<string, int>(new PipelineMiddlewareDelegate<string, int>[]
            {
                async (ctx, next) => await next() + 1
            });

            var response = await execution.OnMiddlewareAsync(
                new ReadOnlyContextGenericStub(),
                () => Task.FromResult(41));

            Assert.Equal(42, response);
        }
    }

    public class ReadOnlyContextStub : IReadOnlyContext<string>
    {
        public string Request => "test";
        public IServiceProvider Services => null;
        public bool Cancelled => false;
        public PipelineState PipelineState => PipelineState.OnTargeting;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }

    public class ReadOnlyContextGenericStub : IReadOnlyContext<string, int>
    {
        public string Request => "test";
        public int Response => 0;
        public IServiceProvider Services => null;
        public bool Cancelled => false;
        public PipelineState PipelineState => PipelineState.OnTargeting;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }
}
