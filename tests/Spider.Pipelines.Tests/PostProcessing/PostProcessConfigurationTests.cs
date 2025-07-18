using Spider.Pipelines.PostProcessing;

namespace Spider.Pipelines.Tests.PostProcessing
{
    public class PostProcessConfigurationTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCollections()
        {
            var config = new PostProcessConfiguration<string>(new ServiceProviderStub());
            Assert.NotNull(config);
        }

        [Fact]
        public void OnFailure_ShouldReturnSelf()
        {
            var config = new PostProcessConfiguration<string>(new ServiceProviderStub());
            var result = config.OnFailure((ctx, args) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void OnSuccess_ShouldReturnSelf()
        {
            var config = new PostProcessConfiguration<string>(new ServiceProviderStub());
            var result = config.OnSuccess((ctx, args) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new PostProcessConfiguration<string>(new ServiceProviderStub());
            var execution = config.BuildExecution();
            Assert.NotNull(execution);
        }
    }

    public class PostProcessConfigurationGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCollections()
        {
            var config = new PostProcessConfiguration<string, int>(new ServiceProviderStub());
            Assert.NotNull(config);
        }

        [Fact]
        public void OnFailure_ShouldReturnSelf()
        {
            var config = new PostProcessConfiguration<string, int>(new ServiceProviderStub());
            var result = config.OnFailure((ctx, args) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void OnSuccess_ShouldReturnSelf()
        {
            var config = new PostProcessConfiguration<string, int>(new ServiceProviderStub());
            var result = config.OnSuccess((ctx, args) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new PostProcessConfiguration<string, int>(new ServiceProviderStub());
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
