using Spider.Pipelines.Core.Internals;
using Spider.Pipelines.Extensions;
using Spider.Pipelines.Parallelization;
using SpiderParallelExecutionMode = Spider.Pipelines.Parallelization.ParallelExecutionMode;

namespace Spider.Pipelines.Tests.Extensions
{
    public class PipelineBuilderExtensionsTests
    {
        [Fact]
        public async Task ShortcutMethods_ShouldConfigureRequestPipeline()
        {
            var events = new List<string>();
            var builder = new PipelineBuilder<string>(new ServiceProviderStub());

            builder
                .PreProcess((ctx, args) =>
                {
                    events.Add("pre");
                    return Task.CompletedTask;
                })
                .UseMiddleware(async (ctx, next) =>
                {
                    events.Add("middleware-before");
                    await next();
                    events.Add("middleware-after");
                })
                .ParallelMode(SpiderParallelExecutionMode.AfterTarget)
                .Parallel((ctx, args) =>
                {
                    events.Add("parallel");
                    return Task.CompletedTask;
                })
                .OnSuccess((ctx, args) =>
                {
                    events.Add("success");
                    return Task.CompletedTask;
                });

            var pipeline = builder.Build((req, token) =>
            {
                events.Add("target");
                return Task.CompletedTask;
            });

            await pipeline.RunAsync("request");

            Assert.Equal(new[] { "pre", "middleware-before", "target", "middleware-after", "parallel", "success" }, events);
        }

        [Fact]
        public async Task ShortcutMethods_ShouldConfigureResponsePipeline()
        {
            var events = new List<string>();
            var builder = new PipelineBuilder<string, int>(new ServiceProviderStub());

            builder
                .PreProcess<string, int>((ctx, args) =>
                {
                    events.Add("pre");
                    return Task.CompletedTask;
                })
                .UseOverride<string, int>((req, token) =>
                {
                    events.Add("override");
                    return Task.FromResult(7);
                })
                .UseMiddleware<string, int>(async (ctx, next) => await next() + 1)
                .OnSuccess<string, int>((ctx, args) =>
                {
                    events.Add($"success:{ctx.Response}");
                    return Task.CompletedTask;
                });

            var pipeline = builder.Build((req, token) =>
            {
                events.Add("target");
                return Task.FromResult(42);
            });

            var response = await pipeline.RunAsync("request");

            Assert.Equal(8, response);
            Assert.Equal(new[] { "pre", "override", "success:8" }, events);
        }

        private sealed class ServiceProviderStub : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }
    }
}
