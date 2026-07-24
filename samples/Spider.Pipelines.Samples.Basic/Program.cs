namespace Spider.Pipelines.Samples.Basic
{
    using Microsoft.Extensions.DependencyInjection;
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;

    /// <summary>
    /// Runs a basic Spider.Pipelines sample.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Configures dependencies and executes a sample pipeline.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task Main()
        {
            await RunGlobalBoundaryExampleAsync();
            await RunFluentBoundaryExampleAsync();
            await RunBridgeBoundaryExampleAsync();
        }

        /// <summary>
        /// Runs a sample pipeline with a globally registered execution boundary.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunGlobalBoundaryExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleOrderService>();
            services.AddSingleton<SampleEventLog>();
            services
                .AddSpider()
                .AddExecutionBoundary<ConsoleBoundary>();

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            log.Write("example: global boundary");

            var receipt = await spider
                .InitBridge<SampleOrderService>()
                .Attach<OrderRequest, OrderReceipt>(builder => ConfigureOrderPipeline(builder, log))
                .ExecuteAsync(
                    service => (request, token) => service.PlaceOrderAsync(request, token),
                    new OrderRequest("SO-1001", 125.50m));

            log.Write($"done: {receipt.ReceiptId} for {receipt.Total:C}");
        }

        /// <summary>
        /// Runs a sample pipeline with delegate callbacks selected through the pipeline fluent API.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunFluentBoundaryExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleOrderService>();
            services.AddSingleton<SampleEventLog>();
            services.AddScoped<ConsoleBoundary>();
            services.AddSpider();

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            log.Write("example: fluent boundary");

            var receipt = await spider
                .InitBridge<SampleOrderService>()
                .Attach<OrderRequest, OrderReceipt>(builder =>
                {
                    builder.AddExecutionBoundary(boundary =>
                    {
                        boundary
                            .OnBegin((ctx, token) =>
                            {
                                log.Write($"fluent-boundary: begin {ctx.RequestType.Name}");
                                return ValueTask.CompletedTask;
                            })
                            .OnComplete((ctx, token) =>
                            {
                                log.Write($"fluent-boundary: complete {ctx.ResponseType?.Name}");
                                return ValueTask.CompletedTask;
                            })
                            .OnFault((ctx, ex, token) =>
                            {
                                log.Write($"fluent-boundary: fault {ex.Message}");
                                return ValueTask.CompletedTask;
                            })
                            .OnCancel((ctx, token) =>
                            {
                                log.Write("fluent-boundary: cancel");
                                return ValueTask.CompletedTask;
                            })
                            .OnDispose(ctx =>
                            {
                                log.Write("fluent-boundary: dispose");
                                return ValueTask.CompletedTask;
                            });
                    });

                    ConfigureOrderPipeline(builder, log);
                })
                .ExecuteAsync(
                    service => (request, token) => service.PlaceOrderAsync(request, token),
                    new OrderRequest("SO-1002", 210m));

            log.Write($"done: {receipt.ReceiptId} for {receipt.Total:C}");
        }

        /// <summary>
        /// Runs a sample pipeline with boundaries selected from the bridge before attaching the pipeline.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunBridgeBoundaryExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleOrderService>();
            services.AddSingleton<SampleEventLog>();
            services.AddScoped<ConsoleBoundary>();
            services.AddScoped<AuditBoundary>();
            services.AddSpider();

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            log.Write("example: bridge boundary");

            var bridge = spider
                .InitBridge<SampleOrderService>()
                .AddExecutionBoundary<ConsoleBoundary>()
                .AddExecutionBoundary<AuditBoundary>()
                .Attach<OrderRequest, OrderReceipt>(builder => ConfigureOrderPipeline(builder, log));

            var receipt = await bridge.ExecuteAsync(
                service => (request, token) => service.PlaceOrderAsync(request, token),
                new OrderRequest("SO-1003", 75m));

            log.Write($"done: {receipt.ReceiptId} for {receipt.Total:C}");
        }

        /// <summary>
        /// Configures the shared order pipeline stages used by the sample executions.
        /// </summary>
        /// <param name="builder">The pipeline builder to configure.</param>
        /// <param name="log">The sample event log.</param>
        private static void ConfigureOrderPipeline(IPipelineBuilder<OrderRequest, OrderReceipt> builder, SampleEventLog log)
        {
            builder
                .PreProcess((ctx, args) =>
                {
                    log.Write($"preprocess: validating order {ctx.Request.OrderId}");
                    return Task.CompletedTask;
                })
                .UseMiddleware(async (ctx, next) =>
                {
                    log.Write("middleware: before handler");
                    var response = await next();
                    log.Write("middleware: after handler");
                    return response;
                })
                .Parallel((ctx, args) =>
                {
                    log.Write("parallel: notifying read model");
                    return Task.CompletedTask;
                })
                .OnSuccess((ctx, args) =>
                {
                    log.Write($"postprocess: receipt {ctx.Response.ReceiptId}");
                    return Task.CompletedTask;
                })
                .OnFailure((ctx, args) =>
                {
                    log.Write($"postprocess: failure {ctx.Exception?.Message}");
                    return Task.CompletedTask;
                });
        }
    }
}
