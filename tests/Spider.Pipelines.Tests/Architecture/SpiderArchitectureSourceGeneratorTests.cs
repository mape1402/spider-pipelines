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
        public void BuildManifest_WhenPipelineUsesDescriptiveMetadata_ShouldGeneratePipelineAndStageMetadata()
        {
            var manifest = GenerateManifest(PipelineMetadataSource);
            var pipelineId = "spider.pipeline:order-request-to-order-receipt";

            var pipeline = Assert.Single(manifest.Components, component => component.Id == pipelineId);
            Assert.Equal("Order placement pipeline", pipeline.DisplayName);
            Assert.Equal("Runs the order handler with operational policy metadata.", pipeline.Metadata["description"]);
            Assert.Equal("Documents the wrapper around order placement.", pipeline.Metadata["purpose"]);
            Assert.Equal("POST /orders", pipeline.Metadata["trigger"]);
            Assert.Equal("OrderService.PlaceAsync", pipeline.Metadata["wraps"]);
            Assert.Equal("OrderRequest", pipeline.Metadata["input"]);
            Assert.Equal("OrderReceipt", pipeline.Metadata["output"]);
            Assert.Equal("validation,audit", pipeline.Metadata["policies"]);
            Assert.Equal("Log and rethrow", pipeline.Metadata["failureBehavior"]);
            Assert.Equal("orders", pipeline.Metadata["module"]);

            var preProcess = Assert.Single(manifest.Components, component => component.Id == pipelineId + ".pre-process");
            Assert.Equal("Validate order", preProcess.DisplayName);
            Assert.Equal("Checks the order before execution.", preProcess.Metadata["description"]);
            Assert.Equal("validation", preProcess.Metadata["tags"]);
            Assert.Equal("Rejects malformed order requests before the handler executes.", preProcess.Metadata["purpose"]);
            Assert.Equal("validation,guard", preProcess.Metadata["policies"]);
            Assert.Equal("order-validation-started", preProcess.Metadata["observability"]);
            Assert.Equal("Expected below 5 ms.", preProcess.Metadata["timeout"]);

            var middleware = Assert.Single(manifest.Components, component => component.Id == pipelineId + ".middleware");
            Assert.Equal("Trace order handler", middleware.DisplayName);
            Assert.Equal("telemetry", middleware.Metadata["tags"]);
            Assert.Equal("OrderService.PlaceAsync", middleware.Metadata["wraps"]);
            Assert.Equal("runtime-tracing,transparent-wrapper", middleware.Metadata["policies"]);
            Assert.Equal("handler-started,handler-completed", middleware.Metadata["observability"]);
            Assert.Equal("Trace and rethrow original exceptions.", middleware.Metadata["failureBehavior"]);
        }

        [Fact]
        public void BuildManifest_WhenBoundaryUsesDescriptiveMetadata_ShouldGenerateBoundaryMetadataAndRelations()
        {
            var manifest = GenerateManifest(BoundaryMetadataSource);
            var pipelineId = "spider.pipeline:order-request-to-order-receipt";

            var boundary = Assert.Single(manifest.Components, component => component.Id == "spider.boundary:http-order-boundary");
            Assert.Equal("HTTP order boundary", boundary.DisplayName);
            Assert.Equal("Spider boundary around HTTP order requests.", boundary.Metadata["description"]);
            Assert.Equal("HTTP request boundary", boundary.Metadata["boundaryType"]);
            Assert.Equal("POST /orders", boundary.Metadata["entryPoint"]);
            Assert.Equal("HTTP", boundary.Metadata["protocol"]);
            Assert.Equal("OrderRequest -> OrderReceipt", boundary.Metadata["contract"]);
            Assert.Equal("auth,validation", boundary.Metadata["policies"]);
            Assert.Equal("jwt", boundary.Metadata["security"]);
            Assert.Equal("traces,metrics", boundary.Metadata["observability"]);
            Assert.Equal("OrderRequest -> OrderReceipt", boundary.Metadata["invokesPipeline"]);

            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "boundary-invokes-pipeline" &&
                relation.SourceId == boundary.Id &&
                relation.TargetId == pipelineId);
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

        [Fact]
        public void BuildManifest_WhenBranchHasRoutes_ShouldGenerateRouteMetadata()
        {
            var manifest = GenerateManifest(BranchFlowSource);
            var branch = Assert.Single(manifest.Components, component => component.Id == "spider.flow:evaluate-risk.002-risk-decision-branch");
            var routes = manifest.Relations
                .Where(relation => relation.Kind == "branch-route" && relation.SourceId == branch.Id)
                .OrderBy(relation => relation.Metadata["order"])
                .ToArray();

            Assert.Equal(2, routes.Length);
            Assert.Contains(manifest.Components, component =>
                component.Id == routes[0].TargetId &&
                component.Kind == "spider.flow-branch-route" &&
                component.DisplayName == "When IsLowRisk" &&
                component.Metadata["condition"] == "IsLowRisk");
            Assert.Contains(manifest.Components, component =>
                component.Id == routes[1].TargetId &&
                component.Kind == "spider.flow-branch-route" &&
                component.DisplayName == "Otherwise");

            var routeStepRelations = manifest.Relations
                .Where(relation => relation.Kind == "route-contains")
                .ToArray();
            Assert.Equal(3, routeStepRelations.Length);
            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "route-next" &&
                relation.SourceId.Contains("score-async", StringComparison.Ordinal) &&
                relation.TargetId.Contains("build-decision", StringComparison.Ordinal));
        }

        [Fact]
        public void BuildManifest_WhenBranchRouteContainsNestedBranch_ShouldGenerateNestedRouteGraph()
        {
            var manifest = GenerateManifest(NestedBranchFlowSource);
            var parentRoute = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow-branch-route" &&
                component.DisplayName == "When IsLowRisk");
            var nestedBranch = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow-branch" &&
                component.DisplayName == "Nested decision");

            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "route-contains" &&
                relation.SourceId == parentRoute.Id &&
                relation.TargetId == nestedBranch.Id);

            var nestedRoutes = manifest.Relations
                .Where(relation => relation.Kind == "branch-route" && relation.SourceId == nestedBranch.Id)
                .ToArray();
            Assert.Equal(2, nestedRoutes.Length);
            Assert.Contains(manifest.Components, component =>
                component.Id == nestedBranch.Id + ".route.01-senior-review" &&
                component.Kind == "spider.flow-branch-route");
            Assert.Contains(manifest.Components, component =>
                component.Id == nestedBranch.Id + ".route.01-senior-review.001-mark-senior-review" &&
                component.Kind == "spider.flow-step");
        }

        [Fact]
        public void BuildManifest_WhenFlowUsesDescriptiveMetadata_ShouldGenerateDocumentationMetadata()
        {
            var manifest = GenerateManifest(MetadataFlowSource);

            var flow = Assert.Single(manifest.Components, component => component.Id == "spider.flow:documented-credit-flow");
            Assert.Equal("Evaluates a credit request with descriptive metadata.", flow.Metadata["description"]);
            Assert.Equal("credit,decision", flow.Metadata["tags"]);
            Assert.Equal("credit-team", flow.Metadata["owner"]);

            var validate = Assert.Single(manifest.Components, component => component.Id == "spider.flow:documented-credit-flow.001-validate-request");
            Assert.Equal("Validate request", validate.DisplayName);
            Assert.Equal("Checks required fields before mapping.", validate.Metadata["description"]);
            Assert.Equal("validation,guard", validate.Metadata["tags"]);

            var branch = Assert.Single(manifest.Components, component => component.Id == "spider.flow:documented-credit-flow.002-score-decision");
            Assert.Equal("Score decision", branch.DisplayName);
            Assert.Equal("Chooses the scoring path.", branch.Metadata["description"]);
            Assert.Equal("branch", branch.Metadata["tags"]);

            var route = Assert.Single(manifest.Components, component => component.Id == branch.Id + ".route.01-low-risk");
            Assert.Equal("Low risk", route.DisplayName);
            Assert.Equal("Fast path for low risk scores.", route.Metadata["description"]);
            Assert.Equal("automatic", route.Metadata["tags"]);

            var routeStep = Assert.Single(manifest.Components, component => component.Id == route.Id + ".001-approve");
            Assert.Equal("Approve", routeStep.DisplayName);
            Assert.Equal("approval", routeStep.Metadata["tags"]);
        }

        [Fact]
        public void BuildManifest_WhenStepMethodDeclaresAnotherFlow_ShouldLinkStepToNestedFlow()
        {
            var manifest = GenerateManifest(NestedFlowSource);
            var mainStep = Assert.Single(manifest.Components, component => component.Id == "spider.flow:main-flow.002-persist-async");
            var nestedFlow = Assert.Single(manifest.Components, component => component.Id == "spider.flow:persist-decision");

            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "invokes-flow" &&
                relation.SourceId == mainStep.Id &&
                relation.TargetId == nestedFlow.Id);
        }

        [Fact]
        public void BuildManifest_WhenPipelineExecuteTargetsMethodWithFlow_ShouldLinkPipelineToTargetFlow()
        {
            var manifest = GenerateManifest(PipelineTargetFlowSource);
            var pipelineId = "spider.pipeline:create-customer-request-to-customer-response";
            var targetStage = Assert.Single(manifest.Components, component => component.Id == pipelineId + ".target");
            var targetFlow = Assert.Single(manifest.Components, component => component.Id == "spider.flow:handle-customer");

            Assert.Equal("HandleAsync", targetStage.Metadata["target"]);
            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "invokes-flow" &&
                relation.SourceId == targetStage.Id &&
                relation.TargetId == targetFlow.Id);
            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "pipeline-invokes-flow" &&
                relation.SourceId == pipelineId &&
                relation.TargetId == targetFlow.Id);
        }

        [Fact]
        public void BuildManifest_WhenBoundaryUsesTypeOfAndDelegateMetadata_ShouldGenerateBoundaryVariantsAndRelations()
        {
            var manifest = GenerateManifest(BoundaryVariantSource);
            var pipeline = Assert.Single(manifest.Components, component => component.Id == "spider.pipeline:order-request-to-order-receipt");
            var flow = Assert.Single(manifest.Components, component => component.Id == "spider.flow:fulfill-order");
            var typeBoundary = Assert.Single(manifest.Components, component => component.Id == "spider.boundary:type-of-boundary");
            var delegateBoundary = Assert.Single(manifest.Components, component => component.DisplayName == "Delegate order boundary");

            Assert.Equal("TypeOfBoundary", typeBoundary.DisplayName);
            Assert.Equal("ArchitectureSample.TypeOfBoundary", typeBoundary.Metadata["boundary"]);

            Assert.Equal("delegate", delegateBoundary.Metadata["boundary"]);
            Assert.Equal("Delegates order execution through a local boundary.", delegateBoundary.Metadata["description"]);
            Assert.Equal("delegate-boundary", delegateBoundary.Metadata["module"]);
            Assert.Equal("FlowRequest -> FlowResponse", delegateBoundary.Metadata["invokesFlow"]);
            Assert.Equal("OrderRequest -> OrderReceipt", delegateBoundary.Metadata["invokesPipeline"]);

            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "boundary-invokes-pipeline" &&
                relation.SourceId == delegateBoundary.Id &&
                relation.TargetId == pipeline.Id);
            Assert.Contains(manifest.Relations, relation =>
                relation.Kind == "boundary-invokes-flow" &&
                relation.SourceId == delegateBoundary.Id &&
                relation.TargetId == flow.Id);
        }

        [Fact]
        public void BuildManifest_WhenAttachHasNoConfigureLambda_ShouldGenerateMinimalPipeline()
        {
            var manifest = GenerateManifest(MinimalAttachSource);
            var pipeline = Assert.Single(manifest.Components, component => component.Id == "spider.pipeline:order-request");

            Assert.Equal("OrderRequest", pipeline.DisplayName);
            Assert.Equal("ArchitectureSample.OrderRequest", pipeline.Metadata["request"]);
            Assert.Equal("false", pipeline.Metadata["hasResponse"]);
            Assert.DoesNotContain("service", pipeline.Metadata.Keys);
            AssertStage(manifest, pipeline.Id + ".pre-process", "0");
            AssertStage(manifest, pipeline.Id + ".target", "1", "False");
        }

        [Fact]
        public void BuildManifest_WhenFlowUsesLambdaStepsAndEmptyValues_ShouldGenerateFallbackMetadata()
        {
            var manifest = GenerateManifest(InlineFlowSource, string.Empty);
            var flow = Assert.Single(manifest.Components, component => component.Id == "spider.flow:inline-only");
            var step = Assert.Single(manifest.Components, component => component.Id == "spider.flow:inline-only.001-lambda");
            var branch = Assert.Single(manifest.Components, component => component.Id == "spider.flow:inline-only.002-branch");

            Assert.Equal("ArchitectureSample.FlowRequest", flow.Metadata["request"]);
            Assert.Equal("false", flow.Metadata["hasResponse"]);
            Assert.Equal("lambda", step.DisplayName);
            Assert.Equal(string.Empty, step.Evidence.Single().FilePath);
            Assert.Equal("Branch", branch.DisplayName);
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

        private const string BranchFlowSource = @"
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class DocumentedFlows
    {
        public void Configure(ISpider spider, RiskRequest request, CancellationToken token)
        {
            _ = spider
                .ComposeFlow<RiskRequest, RiskResponse>(""Evaluate risk"")
                .Then(BuildProfile)
                .Branch<RiskDecision>(branch => branch
                    .When(IsLowRisk, low => low.Then(AutoApprove))
                    .Otherwise(normal => normal
                        .Then(ScoreAsync)
                        .Then(BuildDecision)))
                .Then(ReturnResponse)
                .RunAsync(request, token);
        }

        private static RiskProfile BuildProfile(RiskRequest request) => new RiskProfile();

        private static bool IsLowRisk(RiskProfile profile) => true;

        private static RiskDecision AutoApprove(RiskProfile profile) => new RiskDecision();

        private static Task<RiskScore> ScoreAsync(RiskProfile profile, CancellationToken token) => Task.FromResult(new RiskScore());

        private static RiskDecision BuildDecision(RiskScore score) => new RiskDecision();

        private static RiskResponse ReturnResponse(RiskDecision decision) => new RiskResponse();
    }

    public sealed class RiskRequest { }

    public sealed class RiskProfile { }

    public sealed class RiskScore { }

    public sealed class RiskDecision { }

public sealed class RiskResponse { }
}
";

        private const string NestedBranchFlowSource = @"
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class DocumentedFlows
    {
        public void Configure(ISpider spider, RiskRequest request, CancellationToken token)
        {
            _ = spider
                .ComposeFlow<RiskRequest, RiskResponse>(""Evaluate nested risk"")
                .Then(BuildProfile)
                .Branch<RiskDecision>(branch => branch
                    .When(IsLowRisk, low => low
                        .Then(AutoApprove)
                        .Branch<RiskDecision>(nested => nested
                            .Named(""Nested decision"")
                            .When(RequiresSeniorReview, senior => senior
                                .Named(""Senior review"")
                                .Then(MarkSeniorReview))
                            .Otherwise(clear => clear
                                .Named(""Clear review"")
                                .Then(MarkClearReview))))
                    .Otherwise(normal => normal.Then(CalculateStandardDecision)))
                .Then(ReturnResponse)
                .RunAsync(request, token);
        }

        private static RiskProfile BuildProfile(RiskRequest request) => new RiskProfile();

        private static bool IsLowRisk(RiskProfile profile) => true;

        private static bool RequiresSeniorReview(RiskDecision decision) => true;

        private static RiskDecision AutoApprove(RiskProfile profile) => new RiskDecision();

        private static void MarkSeniorReview(RiskDecision decision) { }

        private static void MarkClearReview(RiskDecision decision) { }

        private static RiskDecision CalculateStandardDecision(RiskProfile profile) => new RiskDecision();

        private static RiskResponse ReturnResponse(RiskDecision decision) => new RiskResponse();
    }

    public sealed class RiskRequest { }

    public sealed class RiskProfile { }

    public sealed class RiskDecision { }

    public sealed class RiskResponse { }
}
";

        private const string MetadataFlowSource = @"
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class DocumentedFlows
    {
        public void Configure(ISpider spider, CreditRequest request, CancellationToken token)
        {
            _ = spider
                .ComposeFlow<CreditRequest, CreditResponse>(""Documented credit flow"")
                .Describe(""Evaluates a credit request with descriptive metadata."")
                .Tags(""credit"", ""decision"")
                .Metadata(""owner"", ""credit-team"")
                .Then(Validate, step => step
                    .Named(""Validate request"")
                    .Describe(""Checks required fields before mapping."")
                    .Tags(""validation"", ""guard""))
                .Branch<CreditDecision>(branch => branch
                    .Named(""Score decision"")
                    .Describe(""Chooses the scoring path."")
                    .Tags(""branch"")
                    .When(IsLowRisk, low => low
                        .Named(""Low risk"")
                        .Describe(""Fast path for low risk scores."")
                        .Tags(""automatic"")
                        .Then(Approve, step => step
                            .Named(""Approve"")
                            .Tags(""approval"")))
                    .Otherwise(normal => normal
                        .Named(""Manual review"")
                        .Then(Review)))
                .Then(ReturnResponse)
                .RunAsync(request, token);
        }

        private static CreditScore Validate(CreditRequest request) => new CreditScore();

        private static bool IsLowRisk(CreditScore score) => true;

        private static CreditDecision Approve(CreditScore score) => new CreditDecision();

        private static CreditDecision Review(CreditScore score) => new CreditDecision();

        private static CreditResponse ReturnResponse(CreditDecision decision) => new CreditResponse();
    }

    public sealed class CreditRequest { }

    public sealed class CreditScore { }

    public sealed class CreditDecision { }

    public sealed class CreditResponse { }
}
";

        private const string PipelineMetadataSource = @"
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;

namespace ArchitectureSample
{
    public sealed class DocumentedPipelines
    {
        public void Configure(ISpider spider)
        {
            spider
                .InitBridge<OrderService>()
                .Attach<OrderRequest, OrderReceipt>(builder => builder
                    .Named(""Order placement pipeline"")
                    .Describe(""Runs the order handler with operational policy metadata."")
                    .Purpose(""Documents the wrapper around order placement."")
                    .Trigger(""POST /orders"")
                    .Wraps(""OrderService.PlaceAsync"")
                    .Input(nameof(OrderRequest))
                    .Output(nameof(OrderReceipt))
                    .Policies(""validation"", ""audit"")
                    .FailureBehavior(""Log and rethrow"")
                    .Module(""orders"")
                    .PreProcess((ctx, args) => Task.CompletedTask, stage => stage
                        .Named(""Validate order"")
                        .Describe(""Checks the order before execution."")
                        .Purpose(""Rejects malformed order requests before the handler executes."")
                        .Policies(""validation"", ""guard"")
                        .Observability(""order-validation-started"")
                        .Timeout(""Expected below 5 ms."")
                        .Tags(""validation""))
                    .UseMiddleware((ctx, next) => next(), stage => stage
                        .Named(""Trace order handler"")
                        .Wraps(""OrderService.PlaceAsync"")
                        .Policies(""runtime-tracing"", ""transparent-wrapper"")
                        .Observability(""handler-started"", ""handler-completed"")
                        .FailureBehavior(""Trace and rethrow original exceptions."")
                        .Tags(""telemetry"")));
        }
    }

    public sealed class OrderService
    {
        public Task<OrderReceipt> PlaceAsync(OrderRequest request, CancellationToken token)
            => Task.FromResult(new OrderReceipt());
    }

    public sealed class OrderRequest { }

    public sealed class OrderReceipt { }
}
";

        private const string BoundaryMetadataSource = @"
using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;

namespace ArchitectureSample
{
    public sealed class DocumentedBoundaries
    {
        public void Configure(IServiceCollection services, ISpider spider)
        {
            services.AddSpider(builder => builder
                .AddExecutionBoundary<HttpOrderBoundary>()
                .Named(""HTTP order boundary"")
                .Describe(""Spider boundary around HTTP order requests."")
                .BoundaryType(""HTTP request boundary"")
                .EntryPoint(""POST /orders"")
                .Protocol(""HTTP"")
                .Operation(""Create order"")
                .Contract<OrderRequest, OrderReceipt>()
                .Policies(""auth"", ""validation"")
                .Security(""jwt"")
                .Observability(""traces"", ""metrics"")
                .InvokesPipeline<OrderRequest, OrderReceipt>());

            spider
                .InitBridge<OrderService>()
                .Attach<OrderRequest, OrderReceipt>(pipeline => { });
        }
    }

    public sealed class HttpOrderBoundary : PipelineExecutionBoundary { }

    public sealed class OrderService { }

    public sealed class OrderRequest { }

    public sealed class OrderReceipt { }
}
";

        private const string NestedFlowSource = @"
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class DocumentedFlows
    {
        public void Configure(ISpider spider, CreditDecisionWorkflow workflow, CreateCustomerRequest request, CancellationToken token)
        {
            _ = spider
                .ComposeFlow<CreateCustomerRequest, CreditDecision>(""Main flow"")
                .Then(BuildDecision)
                .Then(workflow.PersistAsync)
                .RunAsync(request, token);
        }

        private static CreditDecision BuildDecision(CreateCustomerRequest request) => new CreditDecision();
    }

    public sealed class CreditDecisionWorkflow
    {
        private readonly ISpider _spider;

        public CreditDecisionWorkflow(ISpider spider)
        {
            _spider = spider;
        }

        public Task<CreditDecision> PersistAsync(CreditDecision decision, CancellationToken token)
            => _spider
                .ComposeFlow<CreditDecision, CreditDecision>(""Persist decision"")
                .Then(ReturnDecision)
                .RunAsync(decision, token);

        private static CreditDecision ReturnDecision(CreditDecision decision) => decision;
    }

    public sealed class CreateCustomerRequest { }

    public sealed class CreditDecision { }
}
";

        private const string PipelineTargetFlowSource = @"
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class DocumentedFlows
    {
        public void Configure(ISpider spider, CreateCustomerRequest request, CancellationToken token)
        {
            _ = spider
                .InitBridge<CustomerService>()
                .Attach<CreateCustomerRequest, CustomerResponse>(builder => { })
                .ExecuteAsync(service => (item, ct) => service.HandleAsync(item, ct), request, token);
        }
    }

    public sealed class CustomerService
    {
        private readonly ISpider _spider;

        public CustomerService(ISpider spider)
        {
            _spider = spider;
        }

        public Task<CustomerResponse> HandleAsync(CreateCustomerRequest request, CancellationToken token)
            => _spider
                .ComposeFlow<CreateCustomerRequest, CustomerResponse>(""Handle customer"")
                .Then(ReturnResponse)
                .RunAsync(request, token);

        private static CustomerResponse ReturnResponse(CreateCustomerRequest request) => new CustomerResponse();
    }

    public sealed class CreateCustomerRequest { }

    public sealed class CustomerResponse { }
}
";

        private const string BoundaryVariantSource = @"
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class DocumentedBoundaries
    {
        public void Configure(IServiceCollection services, ISpider spider)
        {
            spider
                .InitBridge<OrderService>()
                .AddExecutionBoundary(typeof(TypeOfBoundary))
                .Attach<OrderRequest, OrderReceipt>(builder => { });

            spider
                .InitBridge<OrderService>()
                .AddExecutionBoundary(boundary => boundary
                    .Named(""Delegate order boundary"")
                    .Describe(""Delegates order execution through a local boundary."")
                    .Module(""delegate-boundary"")
                    .InvokesPipeline<OrderRequest, OrderReceipt>()
                    .InvokesFlow<FlowRequest, FlowResponse>());

            spider
                .InitBridge<OrderService>()
                .Attach<OrderRequest, OrderReceipt>(builder => builder
                    .Named(""Order pipeline"")
                    .Wraps<OrderRequest, OrderReceipt, OrderService>()
                    .Input<OrderRequest, OrderReceipt, OrderRequest>()
                    .Output<OrderRequest, OrderReceipt, OrderReceipt>());

            _ = spider
                .ComposeFlow<FlowRequest, FlowResponse>(""Fulfill order"")
                .Then(request => new FlowResponse())
                .RunAsync(new FlowRequest(), CancellationToken.None);
        }
    }

    public sealed class TypeOfBoundary : PipelineExecutionBoundary { }

    public sealed class OrderService { }

    public sealed class OrderRequest { }

    public sealed class OrderReceipt { }

    public sealed class FlowRequest { }

    public sealed class FlowResponse { }
}
";

        private const string MinimalAttachSource = @"
using System;

namespace ArchitectureSample
{
    public sealed class DocumentedPipelines
    {
        public void Configure(ExternalBuilder builder)
        {
            builder.Attach<OrderRequest>(null);
        }
    }

    public sealed class ExternalBuilder
    {
        public void Attach<TRequest>(Action<object> configure) { }
    }

    public sealed class OrderRequest { }
}
";

        private const string InlineFlowSource = @"
using System;
using System.Threading;
using Spider.Pipelines.Core;
using Spider.Pipelines.Flows;

namespace ArchitectureSample
{
    public sealed class InlineFlows
    {
        public void Configure(ISpider spider, FlowRequest request, CancellationToken token)
        {
            _ = spider
                .ComposeFlow<FlowRequest>(""Inline only"")
                .Then(item => { })
                .Branch(branch => { })
                .RunAsync(request, token);
        }
    }

    public static class FlowSyntaxExtensions
    {
        public static ISpiderFlowBuilder<TRequest, TCurrent> Branch<TRequest, TCurrent>(
            this ISpiderFlowBuilder<TRequest, TCurrent> builder,
            Action<object> configure)
            => builder;
    }

    public sealed class FlowRequest { }
}
";
    }
}
