using Spider.Pipelines.Targeting;

namespace Spider.Pipelines.Tests.Targeting
{
    public class TargetConfigurationTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var config = new TargetConfiguration<string>(new ServiceProviderStub());
            Assert.NotNull(config);
        }

        [Fact]
        public void Overrides_ShouldReturnSelf()
        {
            var config = new TargetConfiguration<string>(new ServiceProviderStub());
            var result = config.Overrides((req, token) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void Overrides_WhenHandlerIsNull_ShouldThrow()
        {
            var config = new TargetConfiguration<string>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => config.Overrides(null));
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new TargetConfiguration<string>(new ServiceProviderStub());
            var execution = config.BuildExecution();
            Assert.NotNull(execution);
        }
    }

    public class TargetConfigurationGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var config = new TargetConfiguration<string, int>(new ServiceProviderStub());
            Assert.NotNull(config);
        }

        [Fact]
        public void Overrides_ShouldReturnSelf()
        {
            var config = new TargetConfiguration<string, int>(new ServiceProviderStub());
            var result = config.Overrides((req, token) => Task.FromResult(42));
            Assert.Same(config, result);
        }

        [Fact]
        public void Overrides_WhenHandlerIsNull_ShouldThrow()
        {
            var config = new TargetConfiguration<string, int>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => config.Overrides(null));
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new TargetConfiguration<string, int>(new ServiceProviderStub());
            var execution = config.BuildExecution();
            Assert.NotNull(execution);
        }
    }

    // Stub for IServiceProvider
    public class ServiceProviderStub : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }
}
