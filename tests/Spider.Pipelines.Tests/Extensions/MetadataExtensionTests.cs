using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;
using Spider.Pipelines.Core.Internals;
using Spider.Pipelines.Extensions;

namespace Spider.Pipelines.Tests.Extensions
{
    public sealed class MetadataExtensionTests
    {
        [Fact]
        public void PipelineMetadataExtensions_ForRequestBuilder_ShouldReturnSameBuilder()
        {
            var builder = Substitute.For<IPipelineBuilder<Request>>();

            Assert.Same(builder, builder.Named("name"));
            Assert.Same(builder, builder.Describe("description"));
            Assert.Same(builder, builder.Tags("one", "two"));
            Assert.Same(builder, builder.Metadata("key", "value"));
            Assert.Same(builder, builder.Purpose("purpose"));
            Assert.Same(builder, builder.Trigger("trigger"));
            Assert.Same(builder, builder.Wraps("service"));
            Assert.Same(builder, PipelineBuilderExtensions.Wraps<Request, Service>(builder));
            Assert.Same(builder, builder.Input("input"));
            Assert.Same(builder, PipelineBuilderExtensions.Input<Request, Request>(builder));
            Assert.Same(builder, builder.Output("output"));
            Assert.Same(builder, PipelineBuilderExtensions.Output<Request, Response>(builder));
            Assert.Same(builder, builder.Policies("policy"));
            Assert.Same(builder, builder.FailureBehavior("retry"));
            Assert.Same(builder, PipelineBuilderExtensions.RelatedFlow<Request, Request, Response>(builder));
            Assert.Same(builder, builder.RelatedFlow("flow"));
            Assert.Same(builder, builder.Module("module"));
        }

        [Fact]
        public void PipelineMetadataExtensions_ForResponseBuilder_ShouldReturnSameBuilder()
        {
            var builder = Substitute.For<IPipelineBuilder<Request, Response>>();

            Assert.Same(builder, builder.Named("name"));
            Assert.Same(builder, builder.Describe("description"));
            Assert.Same(builder, builder.Tags("one", "two"));
            Assert.Same(builder, builder.Metadata("key", "value"));
            Assert.Same(builder, builder.Purpose("purpose"));
            Assert.Same(builder, builder.Trigger("trigger"));
            Assert.Same(builder, builder.Wraps("service"));
            Assert.Same(builder, PipelineBuilderExtensions.Wraps<Request, Response, Service>(builder));
            Assert.Same(builder, builder.Input("input"));
            Assert.Same(builder, PipelineBuilderExtensions.Input<Request, Response, Request>(builder));
            Assert.Same(builder, builder.Output("output"));
            Assert.Same(builder, PipelineBuilderExtensions.Output<Request, Response, Response>(builder));
            Assert.Same(builder, builder.Policies("policy"));
            Assert.Same(builder, builder.FailureBehavior("retry"));
            Assert.Same(builder, PipelineBuilderExtensions.RelatedFlow<Request, Response, Request, Response>(builder));
            Assert.Same(builder, builder.RelatedFlow("flow"));
            Assert.Same(builder, builder.Module("module"));
        }

        [Fact]
        public void PipelineShortcutMetadataCallbacks_ShouldInvokeMetadataBuilder()
        {
            var requestBuilder = new PipelineBuilder<Request>(new ServiceProviderStub());
            var responseBuilder = new PipelineBuilder<Request, Response>(new ServiceProviderStub());
            var callbackCount = 0;
            Action<IPipelineMetadataBuilder> metadata = builder =>
            {
                callbackCount++;
                ExerciseMetadataBuilder(builder);
            };

            requestBuilder
                .PreProcess((ctx, args) => Task.CompletedTask, metadata)
                .UseOverride((request, token) => Task.CompletedTask, null, metadata)
                .UseMiddleware((ctx, next) => next(), metadata)
                .Parallel((ctx, args) => Task.CompletedTask, metadata)
                .OnSuccess((ctx, args) => Task.CompletedTask, metadata)
                .OnFailure((ctx, args) => Task.CompletedTask, metadata);

            responseBuilder
                .PreProcess((ctx, args) => Task.CompletedTask, metadata)
                .UseOverride((request, token) => Task.FromResult(new Response()), null, metadata)
                .UseMiddleware(async (ctx, next) => await next(), metadata)
                .Parallel((ctx, args) => Task.CompletedTask, metadata)
                .OnSuccess((ctx, args) => Task.CompletedTask, metadata)
                .OnFailure((ctx, args) => Task.CompletedTask, metadata);

            Assert.Equal(12, callbackCount);
        }

        [Fact]
        public void BoundaryMetadataExtensions_ForSpiderBuilder_ShouldReturnSameBuilder()
        {
            var builder = Substitute.For<ISpiderBuilder>();

            Assert.Same(builder, builder.Named("name"));
            Assert.Same(builder, builder.Describe("description"));
            Assert.Same(builder, builder.Tags("one", "two"));
            Assert.Same(builder, builder.Metadata("key", "value"));
            Assert.Same(builder, builder.Purpose("purpose"));
            Assert.Same(builder, builder.Module("module"));
            Assert.Same(builder, builder.BoundaryType("http"));
            Assert.Same(builder, builder.EntryPoint("POST /customers"));
            Assert.Same(builder, builder.Protocol("HTTP"));
            Assert.Same(builder, builder.Operation("CreateCustomer"));
            Assert.Same(builder, builder.Contract<Request, Response>());
            Assert.Same(builder, builder.Security("auth"));
            Assert.Same(builder, builder.Policies("validation"));
            Assert.Same(builder, builder.FailureBehavior("rollback"));
            Assert.Same(builder, builder.Sla("p95 < 50ms"));
            Assert.Same(builder, builder.Timeout("5s"));
            Assert.Same(builder, builder.Observability("trace"));
            Assert.Same(builder, builder.InvokesPipeline<Request, Response>());
            Assert.Same(builder, builder.InvokesFlow<Request, Response>());
            Assert.Same(builder, builder.External("crm"));
        }

        [Fact]
        public void BoundaryMetadataExtensions_ForDelegateBoundaryConfiguration_ShouldReturnSameConfiguration()
        {
            var configuration = Substitute.For<IExecutionBoundaryConfiguration>();

            Assert.Same(configuration, configuration.Named("name"));
            Assert.Same(configuration, configuration.Describe("description"));
            Assert.Same(configuration, configuration.Tags("one", "two"));
            Assert.Same(configuration, configuration.Metadata("key", "value"));
            Assert.Same(configuration, configuration.Purpose("purpose"));
            Assert.Same(configuration, configuration.Module("module"));
            Assert.Same(configuration, configuration.BoundaryType("http"));
            Assert.Same(configuration, configuration.EntryPoint("POST /customers"));
            Assert.Same(configuration, configuration.Protocol("HTTP"));
            Assert.Same(configuration, configuration.Operation("CreateCustomer"));
            Assert.Same(configuration, configuration.Contract<Request, Response>());
            Assert.Same(configuration, configuration.Security("auth"));
            Assert.Same(configuration, configuration.Policies("validation"));
            Assert.Same(configuration, configuration.FailureBehavior("rollback"));
            Assert.Same(configuration, configuration.Sla("p95 < 50ms"));
            Assert.Same(configuration, configuration.Timeout("5s"));
            Assert.Same(configuration, configuration.Observability("trace"));
            Assert.Same(configuration, configuration.InvokesPipeline<Request, Response>());
            Assert.Same(configuration, configuration.InvokesFlow<Request, Response>());
            Assert.Same(configuration, configuration.External("crm"));
        }

        private static void ExerciseMetadataBuilder(IPipelineMetadataBuilder builder)
        {
            Assert.Same(builder, builder.Named("name"));
            Assert.Same(builder, builder.Describe("description"));
            Assert.Same(builder, builder.Tags("tag"));
            Assert.Same(builder, builder.Metadata("key", "value"));
            Assert.Same(builder, builder.Purpose("purpose"));
            Assert.Same(builder, builder.Trigger("trigger"));
            Assert.Same(builder, builder.Wraps("service"));
            Assert.Same(builder, builder.Wraps<Service>());
            Assert.Same(builder, builder.Input<Request>());
            Assert.Same(builder, builder.Output<Response>());
            Assert.Same(builder, builder.Policies("policy"));
            Assert.Same(builder, builder.FailureBehavior("failure"));
            Assert.Same(builder, builder.RelatedFlow("flow"));
            Assert.Same(builder, builder.RelatedFlow<Request, Response>());
            Assert.Same(builder, builder.Module("module"));
            Assert.Same(builder, builder.BoundaryType("http"));
            Assert.Same(builder, builder.EntryPoint("POST /customers"));
            Assert.Same(builder, builder.Protocol("HTTP"));
            Assert.Same(builder, builder.Operation("CreateCustomer"));
            Assert.Same(builder, builder.Contract<Request, Response>());
            Assert.Same(builder, builder.Security("auth"));
            Assert.Same(builder, builder.Sla("p95 < 50ms"));
            Assert.Same(builder, builder.Timeout("5s"));
            Assert.Same(builder, builder.Observability("trace"));
            Assert.Same(builder, builder.InvokesPipeline<Request, Response>());
            Assert.Same(builder, builder.InvokesFlow<Request, Response>());
            Assert.Same(builder, builder.External("crm"));
        }

        private sealed class ServiceProviderStub : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        public sealed record Request;

        public sealed record Response;

        public sealed class Service
        {
        }
    }
}
