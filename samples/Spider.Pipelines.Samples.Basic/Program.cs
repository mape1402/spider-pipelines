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
            var services = new ServiceCollection();

            services.AddSingleton<SampleOrderService>();
            services.AddSingleton<SampleEventLog>();
            services
                .AddSpider()
                .AddExecutionBoundary<ConsoleBoundary>();

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            var bridge = spider
                .InitBridge<SampleOrderService>()
                .Attach<OrderRequest, OrderReceipt>(builder =>
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
                });

            var receipt = await bridge.ExecuteAsync(
                service => (request, token) => service.PlaceOrderAsync(request, token),
                new OrderRequest("SO-1001", 125.50m));

            log.Write($"done: {receipt.ReceiptId} for {receipt.Total:C}");
        }
    }
}
