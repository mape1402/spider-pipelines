using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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
                component.Evidence.Single().MemberName == "Validate" &&
                component.Metadata["delegate"] == "Validate");
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

        [Fact]
        public void BuildManifest_WhenSourceFilePathIsAbsolute_ShouldGeneratePortableEvidencePaths()
        {
            var projectDirectory = NormalizeTestPath(Path.Combine(Path.GetTempPath(), "spider-source-root"));
            var sourcePath = NormalizeTestPath(Path.Combine(projectDirectory, "Features", "ArchitectureSample.cs"));
            var manifest = GenerateManifest(TestSource, sourcePath, projectDirectory);
            var evidencePaths = manifest.Components
                .SelectMany(component => component.Evidence)
                .Select(evidence => evidence.FilePath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            Assert.NotEmpty(evidencePaths);
            Assert.All(evidencePaths, path =>
            {
                Assert.False(Path.IsPathRooted(path));
                Assert.DoesNotContain(projectDirectory, path, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("\\", path);
            });
            Assert.Contains("Features/ArchitectureSample.cs", evidencePaths);
        }

        [Fact]
        public void BuildManifest_WhenStepUsesMethodFromAnotherFile_ShouldUseMethodDeclarationAsEvidence()
        {
            var projectDirectory = NormalizeTestPath(Path.Combine(Path.GetTempPath(), "spider-external-source-root"));
            var flowPath = NormalizeTestPath(Path.Combine(projectDirectory, "Flows", "FlowDocumentation.cs"));
            var actionPath = NormalizeTestPath(Path.Combine(projectDirectory, "Actions", "ExternalActions.cs"));
            var manifest = GenerateManifest(
                ExternalFlowSource,
                flowPath,
                projectDirectory,
                (ExternalActionsSource, actionPath));

            var step = Assert.Single(manifest.Components, component => component.Id == "spider.flow:external-flow.001-validate");
            var evidence = Assert.Single(step.Evidence);

            Assert.Equal("Validate", evidence.MemberName);
            Assert.Equal("ExternalActions", evidence.TypeName);
            Assert.Equal("Actions/ExternalActions.cs", evidence.FilePath);
        }

        private static void AssertStage(SpiderArchitectureManifest manifest, string id, string count, string hasOverride = null)
        {
            var stage = Assert.Single(manifest.Components, component => component.Id == id);
            Assert.Equal("spider.pipeline-stage", stage.Kind);
            Assert.Equal(count, stage.Metadata["count"]);

            if (hasOverride != null)
                Assert.Equal(hasOverride, stage.Metadata["hasOverride"]);
        }

        private static SpiderArchitectureManifest GenerateManifest(
            string source,
            string filePath = "ArchitectureSample.cs",
            string projectDirectory = null,
            params (string Source, string FilePath)[] additionalSources)
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            var syntaxTrees = new List<SyntaxTree>
            {
                CSharpSyntaxTree.ParseText(source, parseOptions, filePath)
            };

            syntaxTrees.AddRange(additionalSources.Select(additionalSource =>
                CSharpSyntaxTree.ParseText(additionalSource.Source, parseOptions, additionalSource.FilePath)));

            var references = GetMetadataReferences();
            var compilation = CSharpCompilation.Create(
                "ArchitectureSample",
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new SpiderArchitectureSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(
                new[] { generator },
                parseOptions: null,
                optionsProvider: new TestAnalyzerConfigOptionsProvider(projectDirectory));
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

        private static string NormalizeTestPath(string path)
            => path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            private readonly AnalyzerConfigOptions _options;

            public TestAnalyzerConfigOptionsProvider(string projectDirectory)
            {
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(projectDirectory))
                {
                    values["build_property.ProjectDir"] = projectDirectory.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                        ? projectDirectory
                        : projectDirectory + Path.DirectorySeparatorChar;
                    values["build_property.MSBuildProjectDirectory"] = projectDirectory;
                }

                _options = new TestAnalyzerConfigOptions(values);
            }

            public override AnalyzerConfigOptions GlobalOptions => _options;

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
                => _options;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
                => _options;
        }

        private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            private readonly IReadOnlyDictionary<string, string> _values;

            public TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values)
                => _values = values;

            public override bool TryGetValue(string key, out string value)
                => _values.TryGetValue(key, out value);
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

        private const string ExternalFlowSource = @"
using System.Threading;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class FlowDocumentation
    {
        public void Configure(ISpider spider, CreateCustomerRequest request, CancellationToken token)
        {
            _ = spider
                .ComposeFlow<CreateCustomerRequest>(""External flow"")
                .Then(ExternalActions.Validate)
                .RunAsync(request, token);
        }
    }

    public sealed class CreateCustomerRequest { }
}
";

        private const string ExternalActionsSource = @"
using System.Threading;
using System.Threading.Tasks;

namespace ArchitectureSample
{
    public static class ExternalActions
    {
        public static Task Validate(CreateCustomerRequest request, CancellationToken token)
            => Task.CompletedTask;
    }
}
";
    }
}
