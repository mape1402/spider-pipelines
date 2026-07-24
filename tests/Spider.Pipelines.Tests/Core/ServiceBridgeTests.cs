using Spider.Pipelines.Core;
using Spider.Pipelines.Core.Internals;
using Spider.Pipelines.Boundaries;

namespace Spider.Pipelines.Tests.Core
{
    public class ServiceBridgeTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var bridge = new ServiceBridge<object>(new ServiceProviderStub(), new object());
            Assert.NotNull(bridge);
        }
    }

    public class ServiceBridgeGenericTests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var bridge = new ServiceBridge<object, string>(new ServiceProviderStub(), new object(), new PipelineBuilderStub(), Array.Empty<Action<IExecutionBoundaryCollection>>());
            Assert.NotNull(bridge);
        }
    }

    public class ServiceBridgeGeneric2Tests
    {
        [Fact]
        public void Constructor_ShouldInitialize()
        {
            var bridge = new ServiceBridge<object, string, int>(new ServiceProviderStub(), new object(), new PipelineBuilderStub(), Array.Empty<Action<IExecutionBoundaryCollection>>());
            Assert.NotNull(bridge);
        }
    }

    internal class PipelineBuilderStub : IPipelineBuilder
    {
        public IPipelineBuilder<TRequest> Typed<TRequest>()
        {
            throw new NotImplementedException();
        }

        public IPipelineBuilder<TRequest, TResponse> Typed<TRequest, TResponse>()
        {
            throw new NotImplementedException();
        }
    }
}
