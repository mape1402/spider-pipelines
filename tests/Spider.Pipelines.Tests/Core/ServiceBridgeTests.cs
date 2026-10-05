using Spider.Pipelines.Core;
using Spider.Pipelines.Core.Internals;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Tests.Boundaries;
using Microsoft.Extensions.DependencyInjection;

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

        [Fact]
        public void Constructor_WhenArgumentsAreMissing_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceBridge<object>(null, new object()));
            Assert.Throws<ArgumentNullException>(() => new ServiceBridge<object>(new ServiceProviderStub(), null));
        }

        [Fact]
        public async Task ExecuteAsync_WhenRequestPipelineIsAttached_ShouldRunServiceAndBridgeBoundaries()
        {
            var log = new BoundaryEventLog();
            using var provider = ServiceBridgeTestServices.CreateProvider(log);
            var service = provider.GetRequiredService<BridgeTestService>();
            var bridge = new ServiceBridge<BridgeTestService>(provider, service);

            var attached = bridge
                .AddExecutionBoundary(typeof(RecordingBoundary))
                .AddExecutionBoundary(boundary => boundary.OnComplete((ctx, token) =>
                {
                    log.Add("delegate:complete");
                    return ValueTask.CompletedTask;
                }))
                .Attach<string>(builder => { });

            await attached.ExecuteAsync(svc => (request, token) => svc.HandleAsync(request, token), "request");

            Assert.Equal("request", service.HandledRequest);
            Assert.Contains("boundary:begin", log.Events);
            Assert.Contains("boundary:complete", log.Events);
            Assert.Contains("delegate:complete", log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenResponsePipelineIsAttached_ShouldReturnServiceResponse()
        {
            var log = new BoundaryEventLog();
            using var provider = ServiceBridgeTestServices.CreateProvider(log);
            var service = provider.GetRequiredService<BridgeTestService>();
            var bridge = new ServiceBridge<BridgeTestService>(provider, service)
                .AddExecutionBoundary<RecordingBoundary>();

            var attached = bridge.Attach<string, int>(builder => { });

            var response = await attached.ExecuteAsync(svc => (request, token) => svc.CountAsync(request, token), "request");

            Assert.Equal(7, response);
            Assert.Contains("boundary:complete", log.Events);
        }

        [Fact]
        public void AddExecutionBoundary_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var bridge = new ServiceBridge<object>(new ServiceProviderStub(), new object());

            Assert.Throws<ArgumentNullException>(() => bridge.AddExecutionBoundary((Type)null));
            Assert.Throws<InvalidOperationException>(() => bridge.AddExecutionBoundary(typeof(object)));
            Assert.Throws<ArgumentNullException>(() => bridge.AddExecutionBoundary((Action<IExecutionBoundaryConfiguration>)null));
            Assert.Throws<ArgumentNullException>(() => bridge.Attach<string>(null));
            Assert.Throws<ArgumentNullException>(() => bridge.Attach<string, int>(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => bridge.ExecuteAsync<string>(null, "request"));
            Assert.ThrowsAsync<ArgumentNullException>(() => bridge.ExecuteAsync<string, int>(null, "request"));
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

        [Fact]
        public void Constructor_WhenArgumentsAreMissing_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceBridge<object, string>(new ServiceProviderStub(), new object(), null, Array.Empty<Action<IExecutionBoundaryCollection>>()));
            Assert.Throws<ArgumentNullException>(() => new ServiceBridge<object, string>(new ServiceProviderStub(), new object(), new PipelineBuilderStub(), null));
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

        [Fact]
        public void Constructor_WhenArgumentsAreMissing_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceBridge<object, string, int>(new ServiceProviderStub(), new object(), null, Array.Empty<Action<IExecutionBoundaryCollection>>()));
            Assert.Throws<ArgumentNullException>(() => new ServiceBridge<object, string, int>(new ServiceProviderStub(), new object(), new PipelineBuilderStub(), null));
        }
    }

    internal sealed class BridgeTestService
    {
        public string HandledRequest { get; private set; }

        public Task HandleAsync(string request, CancellationToken cancellationToken)
        {
            HandledRequest = request;
            return Task.CompletedTask;
        }

        public Task<int> CountAsync(string request, CancellationToken cancellationToken)
            => Task.FromResult(request.Length);
    }

    internal static class ServiceBridgeTestServices
    {
        public static ServiceProvider CreateProvider(BoundaryEventLog log)
        {
            var services = new ServiceCollection();
            services.AddSingleton(log);
            services.AddTransient<RecordingBoundary>();
            services.AddSingleton<BridgeTestService>();
            return services.BuildServiceProvider();
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
