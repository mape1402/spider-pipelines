using Spider.Pipelines.Core;

namespace Spider.Pipelines.Tests.Core
{
    public class ContextGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var context = new Context<string>("request", new ServiceProviderStub(), CancellationToken.None);
            Assert.NotNull(context);
            Assert.Equal("request", context.Request);
        }
    }

    public class ContextGeneric2Tests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var context = new Context<string, int>("request", new ServiceProviderStub(), CancellationToken.None);
            Assert.NotNull(context);
            Assert.Equal("request", context.Request);
        }
    }

    public class ServiceProviderStub : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }
}
