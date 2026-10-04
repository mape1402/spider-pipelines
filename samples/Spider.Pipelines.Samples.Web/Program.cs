namespace Spider.Pipelines.Samples.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.Generated;
    using Spider.Pipelines.RuntimeTracing;
    using Spider.Pipelines.Web;

    /// <summary>
    /// Hosts the Spider architecture documentation sample.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Starts the sample web application.
        /// </summary>
        /// <param name="args">The application arguments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<ISpiderArchitectureWebRenderer, SpiderArchitectureWebRenderer>();
            builder.Services.AddScoped<RuntimeSampleBoundary>();
            builder.Services.AddScoped<CreditApplicationHandler>();
            builder.Services.AddScoped<CreditDecisionWorkflow>();
            builder.Services.AddScoped<CreditOfferWorkflow>();
            builder.Services.AddSpider(spider => spider
                .AddExecutionBoundary<RuntimeSampleBoundary>()
                .Named("Runtime sample boundary")
                .Describe("Wraps sample HTTP-triggered Spider executions so runtime traces and boundary events stay visible in the UI.")
                .Purpose("Shows where an incoming request crosses into Spider pipeline execution.")
                .BoundaryType("HTTP request boundary")
                .EntryPoint("Sample endpoints under /_spider/sample")
                .Protocol("HTTP")
                .Operation("Execute Spider sample pipeline")
                .Contract<CreditApplicationRequest, CreditOffer>()
                .Security("sample-only", "no-auth")
                .Policies("trace-flush", "fault-capture")
                .FailureBehavior("Flushes trace data before returning the sample fault response.")
                .Sla("Interactive demo request")
                .Timeout("Request cancellation token")
                .Observability("runtime traces", "boundary events")
                .InvokesPipeline<CreditApplicationRequest, CreditOffer>());
            builder.Services.AddSpiderRuntimeTracing(tracing =>
            {
                tracing.QueueCapacity = 1000;
                tracing.UseInMemoryStore(options =>
                {
                    options.MaxTraces = 100;
                    options.MaxEventsPerTrace = 500;
                    options.TraceTtl = TimeSpan.FromMinutes(60);
                });
            });

            var app = builder.Build();

            app.MapGet("/_spider/runtime/traces", async (ISpiderTraceReader reader) =>
                Results.Json(await CreateRuntimePayloadAsync(reader, CancellationToken.None)));

            app.MapPost("/_spider/sample/success", async (HttpContext context) =>
                Results.Json(await ExecuteSampleAsync(
                    context.RequestServices,
                    new CreditApplicationRequest("APP-100", "C-100", 750m),
                    context.RequestAborted)));

            app.MapPost("/_spider/sample/remote", async (HttpContext context) =>
                Results.Json(await ExecuteSampleAsync(
                    context.RequestServices,
                    new CreditApplicationRequest("APP-200", "C-200", 5000m),
                    context.RequestAborted)));

            app.MapPost("/_spider/sample/fault", async (HttpContext context) =>
            {
                try
                {
                    return Results.Json(await ExecuteSampleAsync(
                        context.RequestServices,
                        new CreditApplicationRequest("APP-500", "", 5000m),
                        context.RequestAborted));
                }
                catch (Exception ex)
                {
                    await context.RequestServices.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync(context.RequestAborted);
                    return Results.Json(new { error = ex.Message });
                }
            });

            app.MapPost("/_spider/sample/complex/fast", async (HttpContext context) =>
                Results.Json(await ExecuteComplexSampleAsync(
                    context.RequestServices,
                    new CreditApplicationRequest("OFFER-100", "C-100", 750m),
                    context.RequestAborted)));

            app.MapPost("/_spider/sample/complex/manual", async (HttpContext context) =>
                Results.Json(await ExecuteComplexSampleAsync(
                    context.RequestServices,
                    new CreditApplicationRequest("OFFER-200", "C-200", 2500m),
                    context.RequestAborted)));

            app.MapPost("/_spider/sample/complex/remote", async (HttpContext context) =>
                Results.Json(await ExecuteComplexSampleAsync(
                    context.RequestServices,
                    new CreditApplicationRequest("OFFER-300", "C-300", 12500m),
                    context.RequestAborted)));

            app.MapPost("/_spider/sample/complex/fault", async (HttpContext context) =>
            {
                try
                {
                    return Results.Json(await ExecuteComplexSampleAsync(
                        context.RequestServices,
                        new CreditApplicationRequest("OFFER-500", "C-500", 2800m),
                        context.RequestAborted));
                }
                catch (Exception ex)
                {
                    await context.RequestServices.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync(context.RequestAborted);
                    return Results.Json(new { error = ex.Message });
                }
            });

            app.Use(async (context, next) =>
            {
                if (IsDocumentationPath(context.Request.Path))
                {
                    var manifest = SpiderGeneratedArchitecture.BuildManifest();
                    var renderer = context.RequestServices.GetRequiredService<ISpiderArchitectureWebRenderer>();
                    var reader = context.RequestServices.GetRequiredService<ISpiderTraceReader>();
                    var snapshot = await CreateRuntimeSnapshotAsync(reader, context.RequestAborted);
                    var html = renderer.Render(manifest, new SpiderArchitectureWebOptions
                    {
                        Title = "Spider Sample Architecture",
                        IncludeRuntimeTraces = true,
                        RuntimeTracesEndpoint = "/_spider/runtime/traces",
                        RuntimeTraceSummaries = snapshot.Summaries,
                        RuntimeTraces = snapshot.Traces
                    });

                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(html, context.RequestAborted);
                    return;
                }

                await next();
            });

            await app.RunAsync();
        }

        /// <summary>
        /// Determines whether the requested path should render the documentation UI.
        /// </summary>
        /// <param name="path">The requested path.</param>
        /// <returns><see langword="true"/> when the path renders Spider documentation; otherwise, <see langword="false"/>.</returns>
        private static bool IsDocumentationPath(PathString path)
            => path == "/" || path == "/_spider";

        /// <summary>
        /// Executes the sample Spider pipeline.
        /// </summary>
        /// <param name="services">The request services.</param>
        /// <param name="request">The sample request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The generated credit decision.</returns>
        private static async Task<CreditDecision> ExecuteSampleAsync(
            IServiceProvider services,
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
        {
            var spider = services.GetRequiredService<ISpider>();
            try
            {
                return await spider
                    .InitBridge<CreditApplicationHandler>()
                    .Attach<CreditApplicationRequest, CreditDecision>(builder => builder
                        .Named("Credit decision pipeline")
                        .Describe("Handles the primary credit decision request path with telemetry, side signals, and post-processing.")
                        .Purpose("Keeps the handler execution observable while preserving the service contract.")
                        .Trigger("POST /_spider/sample/{success|remote|fault}")
                        .Wraps("CreditApplicationHandler.HandleAsync")
                        .Input(nameof(CreditApplicationRequest))
                        .Output(nameof(CreditDecision))
                        .Policies("runtime-tracing", "fault-capture")
                        .FailureBehavior("Flushes trace data and keeps the original business exception.")
                        .Module("Credit evaluation")
                        .PreProcess((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Prepare credit request")
                            .Describe("Prepares request context before invoking the credit handler.")
                            .Purpose("Normalizes the request envelope and enriches the runtime trace before the handler starts.")
                            .Policies("request-context", "correlation")
                            .Observability("credit-request-prepared", "trace-enriched")
                            .Timeout("Expected below 10 ms.")
                            .Tags("context", "credit"))
                        .UseMiddleware((ctx, next) => next(), stage => stage
                            .Named("Run credit handler")
                            .Describe("Wraps the handler invocation so the runtime timeline can show the operation.")
                            .Purpose("Makes the handler visible in runtime traces while preserving the original service contract.")
                            .Wraps("CreditApplicationHandler.HandleAsync")
                            .Policies("transparent-wrapper", "runtime-tracing")
                            .Observability("handler-started", "handler-completed", "handler-faulted")
                            .Tags("handler", "telemetry"))
                        .Parallel((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Collect credit side signals")
                            .Describe("Runs sample side work in parallel with the business execution.")
                            .Purpose("Models asynchronous enrichment work that should not block the credit decision.")
                            .Policies("non-blocking", "best-effort")
                            .Observability("side-signal-collected")
                            .FailureBehavior("Trace the side effect failure without replacing the handler outcome.")
                            .Tags("parallel", "signals"))
                        .OnSuccess((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Record credit success")
                            .Describe("Captures the successful outcome for the sample trace.")
                            .Purpose("Adds an audit marker after the credit handler returns successfully.")
                            .Policies("success-only", "audit")
                            .Observability("credit-decision-success")
                            .RelatedFlow<CreditDecision, CreditDecision>()
                            .Tags("success"))
                        .OnFailure((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Record credit failure")
                            .Describe("Captures the failed outcome without changing the thrown exception.")
                            .Purpose("Keeps the original failure visible and attached to the pipeline execution.")
                            .Policies("fault-capture", "rethrow-original")
                            .Observability("credit-decision-fault")
                            .FailureBehavior("Record diagnostic metadata and keep the original exception.")
                            .Tags("failure")))
                    .ExecuteAsync(service => (application, token) => service.HandleAsync(application, token), request, cancellationToken);
            }
            finally
            {
                await services.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Executes the complex credit offer sample pipeline.
        /// </summary>
        /// <param name="services">The request services.</param>
        /// <param name="request">The sample request.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The generated credit offer.</returns>
        private static async Task<CreditOffer> ExecuteComplexSampleAsync(
            IServiceProvider services,
            CreditApplicationRequest request,
            CancellationToken cancellationToken)
        {
            var spider = services.GetRequiredService<ISpider>();
            try
            {
                return await spider
                    .InitBridge<CreditOfferWorkflow>()
                    .Attach<CreditApplicationRequest, CreditOffer>(builder => builder
                        .Named("Complex loan origination pipeline")
                        .Describe("Wraps the complex offer workflow used to exercise nested branches, batches, and linked flows.")
                        .Purpose("Documents the richer sample path from request intake to offer dispatch.")
                        .Trigger("POST /_spider/sample/complex/{scenario}")
                        .Wraps("CreditOfferWorkflow.BuildComplexOfferAsync")
                        .Input(nameof(CreditApplicationRequest))
                        .Output(nameof(CreditOffer))
                        .Policies("runtime-tracing", "fraud-screening", "fault-capture")
                        .FailureBehavior("Fault traces stay attached to the failed flow node and are flushed before returning.")
                        .Module("Loan origination")
                        .PreProcess((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Prepare origination context")
                            .Describe("Builds the sample context used by the loan origination workflow.")
                            .Purpose("Creates the underwriting context used by nested branches and linked flows.")
                            .Policies("context-building", "correlation")
                            .Observability("origination-context-created")
                            .Timeout("Expected below 15 ms.")
                            .Tags("context", "origination"))
                        .UseMiddleware((ctx, next) => next(), stage => stage
                            .Named("Execute origination workflow")
                            .Describe("Invokes the complex workflow under runtime tracing.")
                            .Purpose("Makes the full origination workflow visible as the target operation.")
                            .Wraps("CreditOfferWorkflow.BuildComplexOfferAsync")
                            .Policies("runtime-tracing", "workflow-boundary")
                            .Observability("workflow-started", "workflow-completed", "workflow-faulted")
                            .Tags("handler", "workflow"))
                        .Parallel((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Collect underwriting signals")
                            .Describe("Runs side-channel underwriting signals for the visual trace sample.")
                            .Purpose("Documents parallel underwriting enrichment outside the critical decision path.")
                            .Policies("best-effort", "non-blocking")
                            .External("Underwriting signal provider")
                            .Observability("underwriting-signal-collected")
                            .FailureBehavior("Side-signal failures are trace data; the workflow result remains authoritative.")
                            .Tags("parallel", "underwriting"))
                        .OnSuccess((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Record offer success")
                            .Describe("Records successful offer generation.")
                            .Purpose("Publishes an audit marker after an offer is produced.")
                            .Policies("audit", "success-only")
                            .Observability("offer-generation-success")
                            .RelatedFlow<CreditOffer, CreditOffer>()
                            .Tags("success", "offer"))
                        .OnFailure((ctx, args) => Task.CompletedTask, stage => stage
                            .Named("Record offer failure")
                            .Describe("Records the failed origination path and preserves the original exception.")
                            .Purpose("Keeps fraud, branch, or workflow failures attached to the trace.")
                            .Policies("fault-capture", "rethrow-original")
                            .Observability("offer-generation-fault")
                            .FailureBehavior("Capture the failure path and keep the original exception.")
                            .Tags("failure", "offer")))
                    .ExecuteAsync(service => (application, token) => service.BuildComplexOfferAsync(application, token), request, cancellationToken);
            }
            finally
            {
                await services.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Creates the JSON payload consumed by the runtime UI.
        /// </summary>
        /// <param name="reader">The runtime trace reader.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The runtime payload.</returns>
        private static async Task<object> CreateRuntimePayloadAsync(
            ISpiderTraceReader reader,
            CancellationToken cancellationToken)
        {
            var snapshot = await CreateRuntimeSnapshotAsync(reader, cancellationToken);
            return SpiderRuntimeTraceWebSerializer.CreatePayload(snapshot.Summaries, snapshot.Traces);
        }

        /// <summary>
        /// Creates the current runtime trace snapshot.
        /// </summary>
        /// <param name="reader">The runtime trace reader.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The runtime trace snapshot.</returns>
        private static async Task<RuntimeTraceSnapshot> CreateRuntimeSnapshotAsync(
            ISpiderTraceReader reader,
            CancellationToken cancellationToken)
        {
            var summaries = new List<SpiderTraceSummary>();
            var traces = new List<SpiderTrace>();

            await foreach (var summary in reader.QueryAsync(new SpiderTraceQuery { Limit = 50 }, cancellationToken))
            {
                summaries.Add(summary);
                var trace = await reader.GetAsync(summary.TraceId, cancellationToken);
                if (trace != null)
                    traces.Add(trace);
            }

            return new RuntimeTraceSnapshot(summaries, traces);
        }

        private sealed record RuntimeTraceSnapshot(
            IReadOnlyCollection<SpiderTraceSummary> Summaries,
            IReadOnlyCollection<SpiderTrace> Traces);
    }
}
