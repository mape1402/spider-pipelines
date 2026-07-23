using Spider.Pipelines.Parallelization;

namespace Spider.Pipelines.Tests.Parallelization
{
    public class ParallelConfigurationTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var config = new ParallelConfiguration<string>(new ServiceProviderStub());
            Assert.NotNull(config);
        }

        [Fact]
        public void OnParallel_ShouldReturnSelf()
        {
            var config = new ParallelConfiguration<string>(new ServiceProviderStub());
            var result = config.OnParallel((ctx, args) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void OnParallel_WhenHandlerIsNull_ShouldThrow()
        {
            var config = new ParallelConfiguration<string>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => config.OnParallel(null));
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new ParallelConfiguration<string>(new ServiceProviderStub());
            var execution = config.BuildExecution();
            Assert.NotNull(execution);
        }

    }

    public class ParallelConfigurationGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var config = new ParallelConfiguration<string, int>(new ServiceProviderStub());
            Assert.NotNull(config);
        }

        [Fact]
        public void OnParallel_ShouldReturnSelf()
        {
            var config = new ParallelConfiguration<string, int>(new ServiceProviderStub());
            var result = config.OnParallel((ctx, args) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void OnParallel_WhenHandlerIsNull_ShouldThrow()
        {
            var config = new ParallelConfiguration<string, int>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => config.OnParallel(null));
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new ParallelConfiguration<string, int>(new ServiceProviderStub());
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
