using Spider.Pipelines.Parallelization;
using SpiderParallelExecutionMode = Spider.Pipelines.Parallelization.ParallelExecutionMode;

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
        public void WithMode_ShouldReturnSelf()
        {
            var config = new ParallelConfiguration<string>(new ServiceProviderStub());
            var result = config.WithMode(SpiderParallelExecutionMode.AfterTarget);
            Assert.Same(config, result);
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new ParallelConfiguration<string>(new ServiceProviderStub());
            var execution = config.BuildExecution();
            Assert.NotNull(execution);
        }

        [Fact]
        public void BuildExecution_ShouldUseConfiguredMode()
        {
            var config = new ParallelConfiguration<string>(new ServiceProviderStub());
            var execution = config.WithMode(SpiderParallelExecutionMode.BeforeTarget).BuildExecution();
            Assert.Equal(SpiderParallelExecutionMode.BeforeTarget, execution.Mode);
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
        public void WithMode_ShouldReturnSelf()
        {
            var config = new ParallelConfiguration<string, int>(new ServiceProviderStub());
            var result = config.WithMode(SpiderParallelExecutionMode.AfterTarget);
            Assert.Same(config, result);
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new ParallelConfiguration<string, int>(new ServiceProviderStub());
            var execution = config.BuildExecution();
            Assert.NotNull(execution);
        }

        [Fact]
        public void BuildExecution_ShouldUseConfiguredMode()
        {
            var config = new ParallelConfiguration<string, int>(new ServiceProviderStub());
            var execution = config.WithMode(SpiderParallelExecutionMode.BeforeTarget).BuildExecution();
            Assert.Equal(SpiderParallelExecutionMode.BeforeTarget, execution.Mode);
        }
    }

    // Stub for IServiceProvider
    public class ServiceProviderStub : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }
}
