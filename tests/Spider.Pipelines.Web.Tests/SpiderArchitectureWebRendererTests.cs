using Spider.Pipelines.Architecture;

namespace Spider.Pipelines.Web.Tests
{
    public sealed class SpiderArchitectureWebRendererTests
    {
        [Fact]
        public void Render_WhenManifestIsProvided_ShouldRenderGraphicalDocumentation()
        {
            var renderer = new SpiderArchitectureWebRenderer();

            var html = renderer.Render(CreateManifest());

            Assert.Contains("spider-architecture-app", html);
            Assert.Contains("spider-documentation-app", html);
            Assert.Contains("spider-sidebar", html);
            Assert.Contains("spider-menu", html);
            Assert.Contains("spider-menu-section", html);
            Assert.Contains("spider-menu-icon", html);
            Assert.Contains("spider-menu-text", html);
            Assert.Contains("spider-sidebar-toggle", html);
            Assert.Contains("Collapse navigation", html);
            Assert.Contains("is-sidebar-collapsed", html);
            Assert.Contains("spider:architecture:sidebar-collapsed", html);
            Assert.Contains("spider-topbar", html);
            Assert.Contains("data-menu-view=\"pipelines\"", html);
            Assert.Contains("data-menu-view=\"flows\"", html);
            Assert.Contains("#e6242d", html);
            Assert.Contains("#1d5fbf", html);
            Assert.Contains("spider-pipeline-list", html);
            Assert.Contains("spider-flow-list", html);
            Assert.Contains("spider-content", html);
            Assert.Contains("spider-list-view", html);
            Assert.Contains("spider-process-list", html);
            Assert.Contains("spider-process-accent", html);
            Assert.Contains("spider-process-kind", html);
            Assert.Contains("spider-process-action", html);
            Assert.Contains("spider-process-arrow", html);
            Assert.Contains("grid-template-columns: repeat(auto-fill, minmax(320px, 360px))", html);
            Assert.Contains("grid-auto-rows: 96px", html);
            Assert.Contains("height: 96px", html);
            Assert.Contains("text-overflow: ellipsis", html);
            Assert.Contains("data-open-process", html);
            Assert.Contains("spider-detail-view", html);
            Assert.Contains("spider-process-summary", html);
            Assert.Contains("spider-summary-grid", html);
            Assert.Contains("spider-outline", html);
            Assert.Contains("spider-outline-row", html);
            Assert.Contains("spider-node-detail", html);
            Assert.Contains("Pipelines", html);
            Assert.Contains("Flows", html);
            Assert.Contains("spider-process-graph", html);
            Assert.Contains("spider-architecture-graph", html);
            Assert.Contains("spider-graph-node", html);
            Assert.Contains("is-nested is-route", html);
            Assert.Contains("spider-related-button", html);
            Assert.Contains("spider-node-box", html);
            Assert.Contains("spider-node-accent", html);
            Assert.Contains("data-node-id", html);
            Assert.Contains("data-open-process", html);
            Assert.Contains("spider-edge", html);
            Assert.Contains("spider-graph-legend", html);
            Assert.Contains("data-toggle-inspector", html);
            Assert.Contains("↗ Linked flow", html);
            Assert.Contains("spider-link-dot", html);
            Assert.Contains("Action declared in", html);
            Assert.Contains("Action source file", html);
            Assert.Contains("Configured in", html);
            Assert.Contains("Configuration source file", html);
            Assert.Contains("Action", html);
            Assert.Contains("Fluent call", html);
            Assert.Contains("Branch routes", html);
            Assert.Contains("Opens related flow", html);
            Assert.Contains("branch-route", html);
            Assert.Contains("invokes-flow", html);
            Assert.Contains("spider-manifest-data", html);
            Assert.Contains("Create customer", html);
            Assert.Contains("Persist customer", html);
            Assert.Contains("spider.flow:create-customer", html);
            Assert.Contains("spider.pipeline:create-customer-request-to-customer-response", html);
        }

        [Fact]
        public void Render_WhenOptionalPanelsAreDisabled_ShouldExposeDisabledUiFlags()
        {
            var renderer = new SpiderArchitectureWebRenderer();
            var options = new SpiderArchitectureWebOptions
            {
                IncludeEvidence = false,
                IncludeGraph = false,
                IncludeJsonPanel = false,
                IncludeSearch = false,
                Title = "Service Map"
            };

            var html = renderer.Render(CreateManifest(), options);

            Assert.Contains("data-show-evidence=\"false\"", html);
            Assert.Contains("data-show-graph=\"false\"", html);
            Assert.Contains("data-show-json=\"false\"", html);
            Assert.Contains("data-show-search=\"false\"", html);
            Assert.Contains("<title>Service Map</title>", html);
        }

        [Fact]
        public void Serialize_WhenMetadataContainsClosingScriptTag_ShouldEscapeIt()
        {
            var serializer = new SpiderArchitectureManifestSerializer();
            var manifest = new SpiderArchitectureManifest(
                new[]
                {
                    new SpiderComponentDescriptor(
                        "spider.flow:danger",
                        "spider.flow",
                        "Danger",
                        new Dictionary<string, string>
                        {
                            ["description"] = "</script><script>alert(1)</script>"
                        })
                },
                Array.Empty<SpiderRelationDescriptor>());

            var json = serializer.Serialize(manifest);

            Assert.DoesNotContain("</script>", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\\u003C/script", json);
        }

        private static SpiderArchitectureManifest CreateManifest()
        {
            var flowId = "spider.flow:create-customer";
            var validateId = flowId + ".001-validate";
            var mapId = flowId + ".002-map";
            var branchId = flowId + ".003-customer-branch";
            var routeId = branchId + ".route.01-when-is-priority";
            var routeStepId = routeId + ".001-approve-priority";
            var nestedFlowId = "spider.flow:persist-customer";
            var pipelineId = "spider.pipeline:create-customer-request-to-customer-response";

            return new SpiderArchitectureManifest(
                new[]
                {
                    new SpiderComponentDescriptor(
                        flowId,
                        "spider.flow",
                        "Create customer",
                        new Dictionary<string, string>
                        {
                            ["request"] = "CreateCustomerRequest",
                            ["response"] = "CustomerResponse"
                        },
                        new[]
                        {
                            new SpiderEvidenceDescriptor(
                                "source-generator",
                                "CustomerFlow.cs",
                                12,
                                "CustomerFlow",
                                "CreateAsync")
                        }),
                    new SpiderComponentDescriptor(
                        validateId,
                        "spider.flow-step",
                        "Validate",
                        new Dictionary<string, string>
                        {
                            ["delegate"] = "Validate"
                        }),
                    new SpiderComponentDescriptor(
                        mapId,
                        "spider.flow-step",
                        "Map",
                        new Dictionary<string, string>
                        {
                            ["delegate"] = "Map"
                        }),
                    new SpiderComponentDescriptor(
                        branchId,
                        "spider.flow-branch",
                        "Customer branch",
                        new Dictionary<string, string>
                        {
                            ["branchType"] = "CustomerDecision"
                        }),
                    new SpiderComponentDescriptor(
                        routeId,
                        "spider.flow-branch-route",
                        "When IsPriority",
                        new Dictionary<string, string>
                        {
                            ["routeKind"] = "when",
                            ["condition"] = "IsPriority",
                            ["order"] = "1"
                        }),
                    new SpiderComponentDescriptor(
                        routeStepId,
                        "spider.flow-step",
                        "ApprovePriority",
                        new Dictionary<string, string>
                        {
                            ["delegate"] = "ApprovePriority",
                            ["order"] = "1"
                        }),
                    new SpiderComponentDescriptor(
                        nestedFlowId,
                        "spider.flow",
                        "Persist customer",
                        new Dictionary<string, string>
                        {
                            ["request"] = "CustomerResponse",
                            ["response"] = "CustomerResponse"
                        }),
                    new SpiderComponentDescriptor(
                        pipelineId,
                        "spider.pipeline",
                        "CreateCustomerRequest",
                        new Dictionary<string, string>
                        {
                            ["request"] = "CreateCustomerRequest",
                            ["response"] = "CustomerResponse"
                        }),
                    new SpiderComponentDescriptor(
                        pipelineId + ".middleware",
                        "spider.pipeline-stage",
                        "Middleware",
                        new Dictionary<string, string>
                        {
                            ["stage"] = "middleware",
                            ["count"] = "1",
                            ["order"] = "2"
                        })
                },
                new[]
                {
                    new SpiderRelationDescriptor("flow-contains-validate", flowId, validateId, "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("flow-contains-map", flowId, mapId, "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("flow-contains-branch", flowId, branchId, "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("flow-next", validateId, mapId, "next", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("flow-next-branch", mapId, branchId, "next", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("branch-route", branchId, routeId, "branch-route", new Dictionary<string, string> { ["order"] = "1" }),
                    new SpiderRelationDescriptor("route-contains", routeId, routeStepId, "route-contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("route-invokes-flow", routeStepId, nestedFlowId, "invokes-flow", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("pipeline-contains-middleware", pipelineId, pipelineId + ".middleware", "contains", new Dictionary<string, string>())
                });
        }
    }
}
