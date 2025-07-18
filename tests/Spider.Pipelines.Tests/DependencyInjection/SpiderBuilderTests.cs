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
    }
}
