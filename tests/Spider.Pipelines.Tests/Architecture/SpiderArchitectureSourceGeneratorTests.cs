using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Spider.Pipelines.Analyzers;
using Spider.Pipelines.Architecture;
using Spider.Pipelines.Core;

namespace Spider.Pipelines.Tests.Architecture
{
    public class SpiderArchitectureSourceGeneratorTests
    {
        [Fact]
        public void BuildManifest_WhenComposeFlowIsCompiled_ShouldGenerateFlowMetadata()
        {
            var manifest = GenerateManifest(TestSource);

            var flow = Assert.Single(manifest.Components, component => component.Id == "spider.flow:create-customer");
            Assert.Equal("spider.flow", flow.Kind);
            Assert.Equal("ArchitectureSample.CreateCustomerRequest", flow.Metadata["request"]);
            Assert.Equal("ArchitectureSample.CustomerResponse", flow.Metadata["response"]);

            Assert.Contains(manifest.Components, component =>
                component.Id == "spider.flow-profile:business" &&
                component.Kind == "spider.flow-profile");
            Assert.Equal(5, manifest.Components.Count(component =>
                component.Id.StartsWith("spider.flow:create-customer.", StringComparison.Ordinal) &&
                (component.Kind == "spider.flow-step" ||
                 component.Kind == "spider.flow-condition" ||
                 component.Kind == "spider.flow-branch")));
            Assert.Contains(manifest.Components, component =>
                component.Id == "spider.flow:create-customer.001-validate" &&
                component.Evidence.Single().MemberName == "Validate");
            Assert.Equal(4, manifest.Relations.Count(relation =>
                relation.Kind == "next" &&
                relation.SourceId.StartsWith("spider.flow:create-customer.", StringComparison.Ordinal)));
        }

        [Fact]
        public void BuildManifest_WhenPipelineAttachIsCompiled_ShouldGeneratePipelineMetadata()
        {
            var manifest = GenerateManifest(TestSource);
            var pipelineId = "spider.pipeline:create-customer-request-to-customer-response";

            var pipeline = Assert.Single(manifest.Components, component => component.Id == pipelineId);
            Assert.Equal("spider.pipeline", pipeline.Kind);
            Assert.Equal("ArchitectureSample.CreateCustomerRequest", pipeline.Metadata["request"]);
            Assert.Equal("ArchitectureSample.CustomerResponse", pipeline.Metadata["response"]);

            AssertStage(manifest, $"{pipelineId}.pre-process", "1");
            AssertStage(manifest, $"{pipelineId}.middleware", "1");
            AssertStage(manifest, $"{pipelineId}.target", "1", "True");
            AssertStage(manifest, $"{pipelineId}.parallel", "1");
            AssertStage(manifest, $"{pipelineId}.post-success", "1");
            AssertStage(manifest, $"{pipelineId}.post-failure", "1");
        }

        private static void AssertStage(SpiderArchitectureManifest manifest, string id, string count, string hasOverride = null)
        {
            var stage = Assert.Single(manifest.Components, component => component.Id == id);
            Assert.Equal("spider.pipeline-stage", stage.Kind);
            Assert.Equal(count, stage.Metadata["count"]);

            if (hasOverride != null)
                Assert.Equal(hasOverride, stage.Metadata["hasOverride"]);
        }

        private static SpiderArchitectureManifest GenerateManifest(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest), "ArchitectureSample.cs");
            var references = GetMetadataReferences();
            var compilation = CSharpCompilation.Create(
                "ArchitectureSample",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new SpiderArchitectureSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

            using var stream = new MemoryStream();
            var emitResult = outputCompilation.Emit(stream);

            Assert.True(
                emitResult.Success,
                string.Join(Environment.NewLine, emitResult.Diagnostics.Select(diagnostic => diagnostic.ToString())));

            var assembly = Assembly.Load(stream.ToArray());
            var generatedType = assembly.GetType("Spider.Pipelines.Generated.SpiderGeneratedArchitecture", throwOnError: true);
            var buildManifest = generatedType.GetMethod("BuildManifest", BindingFlags.Public | BindingFlags.Static);
            return (SpiderArchitectureManifest)buildManifest.Invoke(null, Array.Empty<object>());
        }

        private static IReadOnlyCollection<MetadataReference> GetMetadataReferences()
        {
            var trustedPlatformAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();

            trustedPlatformAssemblies.Add(MetadataReference.CreateFromFile(typeof(ISpider).Assembly.Location));
            return trustedPlatformAssemblies;
        }

        private const string TestSource = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class DocumentedFlows
    {
        public void Configure(ISpider spider, CreateCustomerRequest request, CancellationToken token)
        {
            _ = spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>(""Create customer"")
                .UsingProfile(""Business"")
                .Then(Validate)
                .Then(Map)
                .ThenWith<CreateCustomerRequest, Customer>(Save)
                .ContinueIf(IsActive, Flow.Throw(() => new InvalidOperationException(""Inactive customer."")))
                .Then(ReturnResponse)
                .RunAsync(request, token);

            spider
                .InitBridge<CustomerService>()
                .Attach<CreateCustomerRequest, CustomerResponse>(builder => builder
                    .PreProcess((ctx, args) => Task.CompletedTask)
                    .UseMiddleware((ctx, next) => next())
                    .UseOverride((request, ct) => Task.FromResult(new CustomerResponse()))
                    .Parallel((ctx, args) => Task.CompletedTask)
                    .OnSuccess((ctx, args) => Task.CompletedTask)
                    .OnFailure((ctx, args) => Task.CompletedTask));
        }

        private static Task Validate(CreateCustomerRequest request, CancellationToken token) => Task.CompletedTask;

        private static Task<Customer> Map(CreateCustomerRequest request, CancellationToken token) => Task.FromResult(new Customer());

        private static Task Save(CreateCustomerRequest request, Customer customer, CancellationToken token) => Task.CompletedTask;

        private static bool IsActive(Customer customer) => true;

        private static CustomerResponse ReturnResponse(Customer customer) => new CustomerResponse();
    }

    public sealed class CustomerService
    {
        public Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken token)
            => Task.FromResult(new CustomerResponse());
    }

    public sealed class CreateCustomerRequest { }

    public sealed class Customer { }

    public sealed class CustomerResponse { }
}
";
    }
}
