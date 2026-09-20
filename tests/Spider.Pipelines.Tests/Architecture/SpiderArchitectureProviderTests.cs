using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Architecture;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;

namespace Spider.Pipelines.Tests.Architecture
{
    public class SpiderArchitectureProviderTests
    {
        [Fact]
        public async Task GetManifest_WhenFlowIsComposed_ShouldDescribeFlowStepsAndOrder()
        {
            var provider = CreateProvider();
            var spider = provider.GetRequiredService<ISpider>();

            await spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
                .UsingProfile("Business")
                .Then(ValidateAsync)
                .Then(MapAsync)
                .ThenWith<CreateCustomerRequest, Customer>(SaveAsync)
                .Then(ReturnResponse)
                .RunAsync(new CreateCustomerRequest("ada@example.com"), CancellationToken.None);

            var manifest = provider.GetRequiredService<ISpiderArchitectureProvider>().GetManifest();

            var flow = Assert.Single(manifest.Components, component => component.Id == "spider.flow:create-customer");
            Assert.Equal("spider.flow", flow.Kind);
            Assert.Equal(typeof(CreateCustomerRequest).FullName, flow.Metadata["request"]);
            Assert.Equal(typeof(CustomerResponse).FullName, flow.Metadata["response"]);
            Assert.Contains(manifest.Components, component =>
                component.Id == "spider.flow-profile:business" &&
                component.Kind == "spider.flow-profile");
            Assert.Equal(4, manifest.Components.Count(component =>
                component.Id.StartsWith("spider.flow:create-customer.", StringComparison.OrdinalIgnoreCase) &&
                component.Kind == "spider.flow-step"));
            Assert.Equal(3, manifest.Relations.Count(relation =>
                relation.Kind == "next" &&
                relation.SourceId.StartsWith("spider.flow:create-customer.", StringComparison.OrdinalIgnoreCase)));
            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "uses-profile" &&
                relation.SourceId == "spider.flow:create-customer" &&
                relation.TargetId == "spider.flow-profile:business");
        }

        [Fact]
        public void GetManifest_WhenPipelineIsAttached_ShouldDescribePipelineStages()
        {
            var provider = CreateProvider();
            var spider = provider.GetRequiredService<ISpider>();

            spider
                .InitBridge<CustomerService>()
                .Attach<CreateCustomerRequest, CustomerResponse>(builder => builder
                    .PreProcess((_, _) => Task.CompletedTask)
                    .UseMiddleware((_, next) => next())
                    .UseOverride((_, _) => Task.FromResult(new CustomerResponse(true, "override@example.com", "override")))
                    .Parallel((_, _) => Task.CompletedTask)
                    .OnSuccess((_, _) => Task.CompletedTask)
                    .OnFailure((_, _) => Task.CompletedTask));

            var manifest = provider.GetRequiredService<ISpiderArchitectureProvider>().GetManifest();
            var pipelineId = "spider.pipeline:create-customer-request-to-customer-response";

            Assert.Contains(manifest.Components, component =>
                component.Id == pipelineId &&
                component.Kind == "spider.pipeline" &&
                component.Metadata["request"] == typeof(CreateCustomerRequest).FullName &&
                component.Metadata["response"] == typeof(CustomerResponse).FullName);
            Assert.Equal(6, manifest.Components.Count(component =>
                component.Id.StartsWith($"{pipelineId}.", StringComparison.OrdinalIgnoreCase) &&
                component.Kind == "spider.pipeline-stage"));
            AssertStage(manifest, $"{pipelineId}.pre-process", "1");
            AssertStage(manifest, $"{pipelineId}.middleware", "1");
            AssertStage(manifest, $"{pipelineId}.target", "1", "True");
            AssertStage(manifest, $"{pipelineId}.parallel", "1");
            AssertStage(manifest, $"{pipelineId}.post-success", "1");
            AssertStage(manifest, $"{pipelineId}.post-failure", "1");
        }

        private static ServiceProvider CreateProvider()
        {
            var services = new ServiceCollection();
            services.AddScoped<CustomerService>();
            services.AddSpider(builder =>
            {
                builder.AddFlowProfile("Business", profile =>
                {
                    profile.TelemetryEnabled = true;
                    profile.MetricsEnabled = true;
                });
            });

            return services.BuildServiceProvider();
        }

        private static void AssertStage(SpiderArchitectureManifest manifest, string id, string count, string hasOverride = null)
        {
            var stage = Assert.Single(manifest.Components, component => component.Id == id);
            Assert.Equal("spider.pipeline-stage", stage.Kind);
            Assert.Equal(count, stage.Metadata["count"]);

            if (hasOverride != null)
                Assert.Equal(hasOverride, stage.Metadata["hasOverride"]);
        }

        private static Task ValidateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return Task.CompletedTask;
        }

        private static Task<Customer> MapAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new Customer(request.Email));

        private static Task SaveAsync(CreateCustomerRequest request, Customer customer, CancellationToken cancellationToken)
            => Task.CompletedTask;

        private static CustomerResponse ReturnResponse(Customer customer)
            => new(true, customer.Email, "Customer created.");

        private sealed record CreateCustomerRequest(string Email);

        private sealed record Customer(string Email);

        private sealed record CustomerResponse(bool Created, string Email, string Message);

        private sealed class CustomerService
        {
            public Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
                => Task.FromResult(new CustomerResponse(true, request.Email, "Customer created."));
        }
    }
}
