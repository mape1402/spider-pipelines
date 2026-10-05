using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Flows;

namespace Spider.Pipelines.Tests.Flows
{
    public class FlowProfileRegistryTests
    {
        [Fact]
        public void Set_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var registry = new FlowProfileRegistry();

            Assert.Throws<ArgumentException>(() => registry.Set("", new FlowProfileOptions()));
            Assert.Throws<ArgumentNullException>(() => registry.Set("Business", null));
        }

        [Fact]
        public void SetAndTryGet_ShouldStoreProfilesCaseInsensitively()
        {
            var registry = new FlowProfileRegistry();
            var options = new FlowProfileOptions
            {
                TelemetryEnabled = true,
                MetricsEnabled = true
            };

            registry.Set("Business", options);

            Assert.True(registry.TryGet("business", out var resolved));
            Assert.Same(options, resolved);
        }

        [Fact]
        public void TryGet_WhenProfileDoesNotExist_ShouldReturnFalse()
        {
            var registry = new FlowProfileRegistry();

            Assert.False(registry.TryGet("missing", out var resolved));
            Assert.Null(resolved);
        }

        [Fact]
        public void Constructor_WhenRegistrationArgumentsAreInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new FlowProfileRegistration("", new FlowProfileOptions()));
            Assert.Throws<ArgumentNullException>(() => new FlowProfileRegistration("Business", null));
        }

        [Fact]
        public void AddSpider_WhenFlowProfilesAreRegistered_ShouldResolveRegistry()
        {
            var services = new ServiceCollection();
            services.AddSpider(builder =>
            {
                builder.AddFlowProfile("Business", profile =>
                {
                    profile.TelemetryEnabled = true;
                    profile.MetricsEnabled = true;
                });
            });
            using var provider = services.BuildServiceProvider();

            var registry = provider.GetRequiredService<FlowProfileRegistry>();

            Assert.True(registry.TryGet("Business", out var options));
            Assert.True(options.TelemetryEnabled);
            Assert.True(options.MetricsEnabled);
        }
    }
}
