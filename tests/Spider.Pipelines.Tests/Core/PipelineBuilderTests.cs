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
        public void ConfigurationMethods_WhenConfigIsNull_ShouldThrow()
        {
            var builder = new PipelineBuilder<string>(new ServiceProviderStub());

            Assert.Throws<ArgumentNullException>(() => builder.OnTargeting(null));
            Assert.Throws<ArgumentNullException>(() => builder.OnParallel(null));
            Assert.Throws<ArgumentNullException>(() => builder.OnPostProcess(null));
            Assert.Throws<ArgumentNullException>(() => builder.OnMiddleware(null));
        }

        [Fact]
        public void Typed_WhenBuilderTypeDoesNotMatch_ShouldThrow()
        {
            var builder = new PipelineBuilder<string>(new ServiceProviderStub());

            Assert.Throws<NotSupportedException>(() => builder.Typed<int>());
            Assert.Throws<NotSupportedException>(() => builder.Typed<string, int>());
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
        public void ConfigurationMethods_WhenConfigIsNull_ShouldThrow()
        {
            var builder = new PipelineBuilder<string, int>(new ServiceProviderStub());

            Assert.Throws<ArgumentNullException>(() => builder.OnTargeting(null));
            Assert.Throws<ArgumentNullException>(() => builder.OnParallel(null));
            Assert.Throws<ArgumentNullException>(() => builder.OnPostProcess(null));
            Assert.Throws<ArgumentNullException>(() => builder.OnMiddleware(null));
        }

        [Fact]
        public void Build_WhenTargetHandlerIsNull_ShouldThrow()
        {
            var builder = new PipelineBuilder<string, int>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => builder.Build(null));
        }
    }
}
