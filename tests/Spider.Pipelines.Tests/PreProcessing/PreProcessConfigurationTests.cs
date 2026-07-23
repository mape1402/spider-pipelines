using Spider.Pipelines.Core;
using Spider.Pipelines.PreProcessing;

namespace Spider.Pipelines.Tests.PreProcessing
{
    public class PreProcessConfigurationTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var config = new PreProcessConfiguration<string>(new ServiceProviderStub());
            Assert.NotNull(config);
        }

        [Fact]
        public void OnPreProcess_ShouldReturnSelf()
        {
            var config = new PreProcessConfiguration<string>(new ServiceProviderStub());
            var result = config.OnPreProcess((ctx, args) => Task.CompletedTask);
            Assert.Same(config, result);
        }

        [Fact]
        public void OnPreProcess_WhenHandlerIsNull_ShouldThrow()
        {
            var config = new PreProcessConfiguration<string>(new ServiceProviderStub());
            Assert.Throws<ArgumentNullException>(() => config.OnPreProcess(null));
        }

        [Fact]
        public void BuildExecution_ShouldReturnExecutionInstance()
        {
            var config = new PreProcessConfiguration<string>(new ServiceProviderStub());
            var execution = config.BuildExecution();
            Assert.NotNull(execution);
        }
    }

    // Stub for IServiceProvider
    public class ServiceProviderStub : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }

    // Stub for IReadOnlyContext<string>
    public class ReadOnlyContextStub : IReadOnlyContext<string>
    {
        public string Request => "test";
        public IServiceProvider Services => null;
        public bool Cancelled => false;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }
}
