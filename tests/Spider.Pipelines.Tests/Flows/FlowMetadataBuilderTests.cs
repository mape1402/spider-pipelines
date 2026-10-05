using Spider.Pipelines.Flows.Internals;

namespace Spider.Pipelines.Tests.Flows
{
    public class FlowMetadataBuilderTests
    {
        [Fact]
        public void Tags_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var builder = new FlowMetadataBuilder();

            Assert.Throws<ArgumentNullException>(() => builder.Tags(null));
        }

        [Fact]
        public void Metadata_WhenKeyIsInvalid_ShouldThrow()
        {
            var builder = new FlowMetadataBuilder();

            Assert.Throws<ArgumentException>(() => builder.Metadata("", "value"));
        }

        [Fact]
        public void MetadataMethods_ShouldCaptureOnlyNonEmptyValues()
        {
            var builder = new FlowMetadataBuilder();

            builder
                .Named("Named step")
                .Describe(" ")
                .Tags(" alpha ", "", "beta")
                .Metadata("owner", "credit")
                .Metadata("empty", "");

            Assert.Equal("Named step", builder.MetadataValues["name"]);
            Assert.False(builder.MetadataValues.ContainsKey("description"));
            Assert.Equal("alpha,beta", builder.MetadataValues["tags"]);
            Assert.Equal("credit", builder.MetadataValues["owner"]);
            Assert.False(builder.MetadataValues.ContainsKey("empty"));
        }
    }
}
