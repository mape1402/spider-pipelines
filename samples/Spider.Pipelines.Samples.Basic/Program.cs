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
            await RunInvocationBoundaryExampleAsync();
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
                .AddBoundary(typeof(GlobalConsoleBoundary<,>));

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
        /// Runs a sample pipeline with a boundary selected through the pipeline fluent API.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunFluentBoundaryExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleOrderService>();
            services.AddSingleton<SampleEventLog>();
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
                        boundary.OnBegin((ctx, token) =>
                        {
                            log.Write($"fluent boundary: begin {ctx.Request.OrderId}");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnComplete((ctx, token) =>
                        {
                            log.Write($"fluent boundary: complete {ctx.Response.ReceiptId}");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnFault((ctx, ex, token) =>
                        {
                            log.Write($"fluent boundary: fault {ex.GetType().Name}");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnCancel((ctx, token) =>
                        {
                            log.Write("fluent boundary: cancel");
                            return ValueTask.CompletedTask;
                        });
                        boundary.OnDispose(ctx =>
                        {
                            log.Write("fluent boundary: dispose");
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
        /// Runs a sample pipeline with a boundary passed directly to one execution.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunInvocationBoundaryExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleOrderService>();
            services.AddSingleton<SampleEventLog>();
            services.AddScoped<InvocationConsoleBoundary>();
            services.AddSpider();

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            log.Write("example: invocation boundary");

            var bridge = spider
                .InitBridge<SampleOrderService>()
                .Attach<OrderRequest, OrderReceipt>(builder => ConfigureOrderPipeline(builder, log));

            var receipt = await bridge.ExecuteAsync(
                service => (request, token) => service.PlaceOrderAsync(request, token),
                new OrderRequest("SO-1003", 75m),
                execution => execution.AddExecutionBoundary<InvocationConsoleBoundary>());

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
