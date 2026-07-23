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

            Assert.Equal(new[] { "pre", "target", "parallel", "success" }, events);
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

            Assert.Equal(7, response);
            Assert.Equal(new[] { "pre", "override", "success:7" }, events);
        }

        private sealed class ServiceProviderStub : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }
    }
}
