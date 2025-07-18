using Spider.Pipelines.Core.Internals;

namespace Spider.Pipelines.Tests.Core
{
    public class PipelineBuilderTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var builder = new PipelineBuilder<string>(new ServiceProviderStub());
            Assert.NotNull(builder);
        }
    }

    public class PipelineBuilderGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var builder = new PipelineBuilder<string, int>(new ServiceProviderStub());
            Assert.NotNull(builder);
        }
    }
}
