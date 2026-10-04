using Spider.Pipelines.Architecture;
using Spider.Pipelines.RuntimeTracing;

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
            Assert.Contains("spider-logo-svg", html);
            Assert.Contains("spider-menu-icon", html);
            Assert.Contains("spider-menu-svg", html);
            Assert.Contains("spider-menu-text", html);
            Assert.Contains("spider-sidebar-toggle", html);
            Assert.Contains("Collapse navigation", html);
            Assert.Contains("is-sidebar-collapsed", html);
            Assert.Contains("spider:architecture:sidebar-collapsed", html);
            Assert.Contains("data-theme=\"light\"", html);
            Assert.Contains("spider-theme-toggle", html);
            Assert.Contains("spider-theme-toggle-icon", html);
            Assert.Contains("spider-theme-icon-sun", html);
            Assert.Contains("spider-theme-icon-moon", html);
            Assert.Contains("spider-theme-toggle-label", html);
            Assert.Contains("Use dark mode", html);
            Assert.Contains("Use light mode", html);
            Assert.Contains("themeToggleLabel.textContent = isDark ? \"Light\" : \"Dark\"", html);
            Assert.Contains("spider:architecture:theme", html);
            Assert.Contains(".spider-shell[data-theme=\"dark\"]", html);
            Assert.Contains("setTheme(readThemePreference())", html);
            Assert.Contains("spider-topbar", html);
            Assert.Contains("spider-topbar-actions", html);
            Assert.Contains("is-process-detail", html);
            Assert.Contains("data-menu-view=\"pipelines\"", html);
            Assert.Contains("data-menu-view=\"flows\"", html);
            Assert.Contains("data-menu-view=\"boundaries\"", html);
            Assert.Contains("#e6242d", html);
            Assert.Contains("#1d5fbf", html);
            Assert.Contains("spider-pipeline-list", html);
            Assert.Contains("spider-flow-list", html);
            Assert.Contains("spider-boundary-list", html);
            Assert.Contains("spider-content", html);
            Assert.Contains("spider-list-view", html);
            Assert.Contains("spider-process-list", html);
            Assert.Contains("spider-process-accent", html);
            Assert.Contains("spider-process-kind", html);
            Assert.Contains("spider-process-action", html);
            Assert.Contains("spider-process-arrow", html);
            Assert.Contains("grid-template-columns: repeat(auto-fill, minmax(min(340px, 100%), 1fr))", html);
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
            Assert.Contains("Boundaries", html);
            Assert.Contains("spider-boundary-count", html);
            Assert.Contains("spider.boundary:http-orders", html);
            Assert.Contains("HTTP orders boundary", html);
            Assert.Contains("boundary-invokes-pipeline", html);
            Assert.Contains("spider-process-graph", html);
            Assert.Contains("Flowchart", html);
            Assert.Contains("spider-flowchart", html);
            Assert.Contains("spider-flowchart-node", html);
            Assert.Contains("spider-flowchart-routes", html);
            Assert.Contains("spider-flowchart-route-body", html);
            Assert.Contains("spider-flowchart-join", html);
            Assert.Contains("renderProcessGraph", html);
            Assert.Contains("renderFlowchart", html);
            Assert.Contains("getFlowchartRoleLabel", html);
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
            Assert.Contains("Pipeline graph", html);
            Assert.Contains("Pipeline stage legend", html);
            Assert.Contains("spider-pipeline-legend-icon", html);
            Assert.Contains("renderPipelineLegendItem(\"boundary\", \"Boundary\")", html);
            Assert.Contains("spider-pipeline-legend-svg", html);
            Assert.Contains("spider-pipeline-legend-mark", html);
            Assert.Contains("renderRoleGlyphShape", html);
            Assert.Contains("getGraphEntryBoundaries", html);
            Assert.Contains("is-boundary", html);
            Assert.Contains("Entry boundaries", html);
            Assert.Contains("Open boundary", html);
            Assert.Contains("is-pipeline-pre", html);
            Assert.Contains("is-pipeline-middleware", html);
            Assert.Contains("is-pipeline-target", html);
            Assert.Contains("is-pipeline-success", html);
            Assert.Contains("is-pipeline-failure", html);
            Assert.Contains("spider-node-stage-pill", html);
            Assert.Contains("spider-node-role-pill", html);
            Assert.Contains("getGraphRoleMarker", html);
            Assert.Contains("spider-stage-summary", html);
            Assert.Contains("Stage summary", html);
            Assert.Contains("post-failure", html);
            Assert.Contains("request-normalization", html);
            Assert.Contains("trace-enrichment", html);
            Assert.Contains("spider-graph-actions", html);
            Assert.Contains("data-process-graph-maximize", html);
            Assert.Contains("spider-graph-maximize-button", html);
            Assert.Contains("spider-graph-maximize-icon", html);
            Assert.Contains("is-graph-maximized", html);
            Assert.Contains("Back to Flows", html);
            Assert.Contains("Back to Pipelines", html);
            Assert.Contains("setProcessDetailShell", html);
            Assert.Contains("setProcessGraphMaximized", html);
            Assert.Contains("preserveProcessGraphScroll", html);
            Assert.Contains("data-toggle-inspector", html);
            Assert.Contains("Linked flow", html);
            Assert.Contains("spider-flowchart-link", html);
            Assert.Contains("spider-link-dot", html);
            Assert.Contains("spider-chip tag", html);
            Assert.Contains("spider-graph-tooltip", html);
            Assert.Contains("data-tooltip-name", html);
            Assert.Contains("data-tooltip-description", html);
            Assert.Contains("data-tooltip-tags", html);
            Assert.Contains("Validate request", html);
            Assert.Contains("Creates a customer and persists the result.", html);
            Assert.Contains("customer", html);
            Assert.Contains("validation", html);
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
            Assert.Contains("data-show-runtime=\"false\"", html);
            Assert.Contains("<title>Service Map</title>", html);
        }

        [Fact]
        public void Render_WhenRuntimeTracesAreEnabled_ShouldRenderRuntimeNavigationAndTraceData()
        {
            var renderer = new SpiderArchitectureWebRenderer();
            var options = new SpiderArchitectureWebOptions
            {
                IncludeRuntimeTraces = true,
                RuntimeTracesEndpoint = "/_spider/runtime/traces",
                RuntimeTraceSummaries = new[]
                {
                    new SpiderTraceSummary
                    {
                        TraceId = "trace-1",
                        RootDisplayName = "Create customer",
                        RequestType = "CustomerCommand",
                        Status = SpiderTraceStatus.Completed,
                        StartedAt = DateTimeOffset.UtcNow,
                        EventCount = 2
                    }
                },
                RuntimeTraces = new[]
                {
                    new SpiderTrace
                    {
                        TraceId = "trace-1",
                        Status = SpiderTraceStatus.Completed,
                        StartedAt = DateTimeOffset.UtcNow,
                        Events = new[]
                        {
                            new SpiderTraceEvent
                            {
                                TraceId = "trace-1",
                                SpanId = "span-1",
                                DisplayName = "Create customer",
                                Operation = "Flow",
                                Kind = SpiderTraceEventKind.FlowStarted,
                                Status = SpiderTraceStatus.Running,
                                Timestamp = DateTimeOffset.UtcNow,
                                Tags = new Dictionary<string, string>
                                {
                                    ["customer"] = "customer"
                                },
                                Metadata = new Dictionary<string, string>
                                {
                                    ["description"] = "Creates the customer response.",
                                    ["tags"] = "customer,flow"
                                }
                            },
                            new SpiderTraceEvent
                            {
                                TraceId = "trace-1",
                                SpanId = "span-1",
                                DisplayName = "Create customer",
                                Operation = "Flow",
                                Kind = SpiderTraceEventKind.FlowCompleted,
                                Status = SpiderTraceStatus.Completed,
                                Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(42),
                                Duration = TimeSpan.FromMilliseconds(42),
                                Tags = new Dictionary<string, string>
                                {
                                    ["customer"] = "customer"
                                },
                                Metadata = new Dictionary<string, string>
                                {
                                    ["description"] = "Creates the customer response.",
                                    ["tags"] = "customer,flow"
                                }
                            }
                        }
                    }
                }
            };

            var html = renderer.Render(CreateManifest(), options);

            Assert.Contains("data-show-runtime=\"true\"", html);
            Assert.Contains("data-runtime-endpoint=\"/_spider/runtime/traces\"", html);
            Assert.Contains("spider-runtime-trace-data", html);
            Assert.Contains("data-menu-view=\"runtime\"", html);
            Assert.Contains("Runtime traces", html);
            Assert.Contains("spider-runtime-import", html);
            Assert.Contains("spider-runtime-export", html);
            Assert.Contains("spider-runtime-import-input", html);
            Assert.Contains("data-import-runtime-trace", html);
            Assert.Contains("data-export-runtime-trace", html);
            Assert.Contains("Import trace", html);
            Assert.Contains("Export trace", html);
            Assert.Contains("is-runtime-list", html);
            Assert.Contains("spider-runtime-execution-list", html);
            Assert.Contains("spider-runtime-row", html);
            Assert.Contains("spider-runtime-overview", html);
            Assert.Contains("spider-runtime-workspace", html);
            Assert.Contains("spider-runtime-visual-panel", html);
            Assert.Contains("spider-runtime-timeline-panel", html);
            Assert.Contains("spider-runtime-context", html);
            Assert.Contains("grid-template-rows: minmax(0, 1fr)", html);
            Assert.Contains("spider-runtime-graph-panel", html);
            Assert.Contains("spider-runtime-node-detail", html);
            Assert.Contains("spider-runtime-timeline-track", html);
            Assert.Contains("spider-runtime-timeline-item", html);
            Assert.Contains("spider-runtime-timeline-number", html);
            Assert.Contains("spider-runtime-timeline-card", html);
            Assert.Contains("data-runtime-item-key", html);
            Assert.Contains("Execution story line", html);
            Assert.Contains("Runtime order, nested spans, selected branch, and faults", html);
            Assert.Contains("data-runtime-visual-view", html);
            Assert.Contains("data-runtime-visual-maximize", html);
            Assert.Contains("Story line", html);
            Assert.Contains("Flow", html);
            Assert.Contains("Maximize", html);
            Assert.Contains("Restore", html);
            Assert.Contains("Execution graph", html);
            Assert.Contains("Not run", html);
            Assert.Contains("is-runtime-completed", html);
            Assert.Contains("is-runtime-faulted", html);
            Assert.Contains("is-runtime-not-executed", html);
            Assert.Contains("Raw events", html);
            Assert.Contains("spider-topbar-back", html);
            Assert.Contains("setRuntimeTraceShell", html);
            Assert.Contains("data-open-raw-events", html);
            Assert.Contains("spider-runtime-visual-actions", html);
            Assert.Contains("spider-runtime-raw-button", html);
            Assert.Contains("spider-runtime-maximize-button", html);
            Assert.Contains("spider-runtime-maximize-icon", html);
            Assert.Contains("is-visual-maximized", html);
            Assert.Contains("spider-modal-backdrop", html);
            Assert.Contains("spider-modal-panel", html);
            Assert.Contains("height: min(780px, calc(100vh - 48px));", html);
            Assert.Contains("spider-modal-search", html);
            Assert.Contains("spider-runtime-raw-search", html);
            Assert.Contains("spider-runtime-raw-list", html);
            Assert.Contains("renderRuntimeRawEventList", html);
            Assert.Contains("filterRuntimeRawEvents", html);
            Assert.Contains("createRuntimeEventSearchText", html);
            Assert.Contains("formatRuntimeRawEventCount", html);
            Assert.Contains("openRuntimeRawEventsModal", html);
            Assert.Contains("closeRuntimeRawEventsModal", html);
            Assert.Contains("spider-runtime-event-top", html);
            Assert.Contains("spider-runtime-event-body", html);
            Assert.Contains("spider-runtime-event-meta", html);
            Assert.Contains("spider-status-chip", html);
            Assert.Contains("buildRuntimeSpans", html);
            Assert.Contains("kind.endsWith(\"Completed\")", html);
            Assert.Contains("createRuntimeOverview", html);
            Assert.Contains("createRuntimeGraphContext", html);
            Assert.Contains("resolveRuntimeSelectedProcess", html);
            Assert.Contains("findRuntimeAncestorProcess", html);
            Assert.Contains("itemKind !== \"spider.flow\" && itemKind !== \"spider.pipeline\"", html);
            Assert.Contains("resolveRuntimeComponentId", html);
            Assert.Contains("resolveRuntimeMarkerComponentId", html);
            Assert.Contains("getRuntimeMetadataValue(marker, \"route\")", html);
            Assert.Contains("setRuntimeVisualView", html);
            Assert.Contains("setRuntimeVisualMaximized", html);
            Assert.Contains("preserveRuntimeVisualScroll", html);
            Assert.Contains("showList(\"runtime\");", html);
            Assert.Contains("selectRuntimeItem", html);
            Assert.Contains("refreshRuntimeSelectionDetail", html);
            Assert.Contains("selectRuntimeGraphNode", html);
            Assert.Contains("exportCurrentRuntimeTrace", html);
            Assert.Contains("importRuntimeTraceFile", html);
            Assert.Contains("extractRuntimeTraceImports", html);
            Assert.Contains("mergeImportedRuntimeData", html);
            Assert.Contains("spider-runtime-trace", html);
            Assert.Contains("Imported", html);
            Assert.Contains("renderRuntimeSpan", html);
            Assert.Contains("renderRuntimeTimeline", html);
            Assert.Contains("renderRuntimeItemDetail", html);
            Assert.Contains("getRuntimeDisplayName", html);
            Assert.Contains("createRuntimeTooltip", html);
            Assert.Contains("spider-runtime-description", html);
            Assert.Contains("spider-runtime-tag", html);
            Assert.Contains("preserveMainScroll", html);
            Assert.Contains("trace-1", html);
            Assert.Contains("Create customer", html);
            Assert.Contains("Creates the customer response.", html);
            Assert.Contains("refreshRuntimeData", html);
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
                            ["response"] = "CustomerResponse",
                            ["description"] = "Creates a customer and persists the result.",
                            ["tags"] = "customer,write"
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
                            ["delegate"] = "Validate",
                            ["name"] = "Validate request",
                            ["description"] = "Checks whether the customer request is valid.",
                            ["tags"] = "validation"
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
                        pipelineId + ".pre-process",
                        "spider.pipeline-stage",
                        "Pre-process",
                        new Dictionary<string, string>
                        {
                            ["stage"] = "pre-process",
                            ["count"] = "1",
                            ["order"] = "1",
                            ["purpose"] = "Creates a normalized request envelope before execution.",
                            ["policies"] = "request-normalization,correlation",
                            ["observability"] = "trace-enrichment",
                            ["timeout"] = "Expected below 5 ms."
                        }),
                    new SpiderComponentDescriptor(
                        pipelineId + ".middleware",
                        "spider.pipeline-stage",
                        "Middleware",
                        new Dictionary<string, string>
                        {
                            ["stage"] = "middleware",
                            ["count"] = "1",
                            ["order"] = "2",
                            ["wraps"] = "CustomerService.CreateAsync",
                            ["policies"] = "runtime-tracing,transparent-wrapper",
                            ["observability"] = "handler-started,handler-completed"
                        }),
                    new SpiderComponentDescriptor(
                        pipelineId + ".target",
                        "spider.pipeline-stage",
                        "Target",
                        new Dictionary<string, string>
                        {
                            ["stage"] = "target",
                            ["count"] = "1",
                            ["hasOverride"] = "true",
                            ["order"] = "3"
                        }),
                    new SpiderComponentDescriptor(
                        pipelineId + ".post-success",
                        "spider.pipeline-stage",
                        "Post-process success",
                        new Dictionary<string, string>
                        {
                            ["stage"] = "post-success",
                            ["count"] = "1",
                            ["order"] = "5"
                        }),
                    new SpiderComponentDescriptor(
                        pipelineId + ".post-failure",
                        "spider.pipeline-stage",
                        "Post-process failure",
                        new Dictionary<string, string>
                        {
                            ["stage"] = "post-failure",
                            ["count"] = "1",
                            ["order"] = "6"
                        }),
                    new SpiderComponentDescriptor(
                        "spider.boundary:http-orders",
                        "spider.boundary",
                        "HTTP orders boundary",
                        new Dictionary<string, string>
                        {
                            ["boundary"] = "HttpOrdersBoundary",
                            ["boundaryType"] = "HTTP request boundary",
                            ["entryPoint"] = "POST /orders",
                            ["protocol"] = "HTTP",
                            ["contract"] = "CreateCustomerRequest -> CustomerResponse",
                            ["description"] = "Documents the entry point that invokes the customer pipeline.",
                            ["policies"] = "auth,validation"
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
                    new SpiderRelationDescriptor("pipeline-contains-pre-process", pipelineId, pipelineId + ".pre-process", "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("pipeline-contains-middleware", pipelineId, pipelineId + ".middleware", "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("pipeline-contains-target", pipelineId, pipelineId + ".target", "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("pipeline-contains-success", pipelineId, pipelineId + ".post-success", "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("pipeline-contains-failure", pipelineId, pipelineId + ".post-failure", "contains", new Dictionary<string, string>()),
                    new SpiderRelationDescriptor("boundary-invokes-pipeline", "spider.boundary:http-orders", pipelineId, "boundary-invokes-pipeline", new Dictionary<string, string>())
                });
        }
    }
}
