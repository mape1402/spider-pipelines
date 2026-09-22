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
            builder.Services.AddSpider(spider => spider.AddExecutionBoundary<RuntimeSampleBoundary>());
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
                        .PreProcess((ctx, args) => Task.CompletedTask)
                        .UseMiddleware((ctx, next) => next())
                        .Parallel((ctx, args) => Task.CompletedTask)
                        .OnSuccess((ctx, args) => Task.CompletedTask)
                        .OnFailure((ctx, args) => Task.CompletedTask))
                    .ExecuteAsync(service => (application, token) => service.HandleAsync(application, token), request, cancellationToken);
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
