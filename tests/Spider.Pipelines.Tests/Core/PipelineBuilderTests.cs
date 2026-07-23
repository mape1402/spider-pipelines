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

        [Fact]
        public void OnPreProcess_WhenConfigIsNull_ShouldThrow()
        {
            var builder = new PipelineBuilder<string>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => builder.OnPreProcess(null));
        }

        [Fact]
        public void Build_WhenTargetHandlerIsNull_ShouldThrow()
        {
            var builder = new PipelineBuilder<string>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => builder.Build(null));
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

        [Fact]
        public void OnPreProcess_WhenConfigIsNull_ShouldThrow()
        {
            var builder = new PipelineBuilder<string, int>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => builder.OnPreProcess(null));
        }

        [Fact]
        public void Build_WhenTargetHandlerIsNull_ShouldThrow()
        {
            var builder = new PipelineBuilder<string, int>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => builder.Build(null));
        }
    }
}
