using Microsoft.Extensions.DependencyInjection;

namespace Spider.Pipelines.Tests.DependencyInjection
{
    public class SpiderBuilderTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var services = new ServiceCollection();
            var builder = new SpiderBuilder(services);
            Assert.NotNull(builder);
            Assert.Equal(services, builder.Services);
        }

        [Fact]
        public void AddSpider_WhenConfigureIsNull_ShouldThrow()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.AddSpider(null));
        }

        [Fact]
        public void AddSpiderRuntimeTracing_WhenServicesAreNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => ServicesExtensions.AddSpiderRuntimeTracing(null, _ => { }));
        }

        [Fact]
        public void AddExecutionBoundary_WhenBuilderIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => ServicesExtensions.AddExecutionBoundary<RecordingBoundary>(null));
        }

        [Fact]
        public void AddFlowProfile_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var services = new ServiceCollection();
            var builder = services.AddSpider();

            Assert.Throws<ArgumentNullException>(() => ServicesExtensions.AddFlowProfile(null, "Business", _ => { }));
            Assert.Throws<ArgumentNullException>(() => builder.AddFlowProfile("Business", null));
        }

        private sealed class RecordingBoundary : Spider.Pipelines.Boundaries.IPipelineExecutionBoundary
        {
            public ValueTask BeginAsync(Spider.Pipelines.Boundaries.PipelineExecutionContext context, CancellationToken cancellationToken)
                => ValueTask.CompletedTask;

            public ValueTask CompleteAsync(Spider.Pipelines.Boundaries.PipelineExecutionContext context, CancellationToken cancellationToken)
                => ValueTask.CompletedTask;

            public ValueTask FaultAsync(Spider.Pipelines.Boundaries.PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
                => ValueTask.CompletedTask;

            public ValueTask CancelAsync(Spider.Pipelines.Boundaries.PipelineExecutionContext context, CancellationToken cancellationToken)
                => ValueTask.CompletedTask;
        }
    }
}
