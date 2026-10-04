namespace Spider.Pipelines.Samples.Basic
{
    using Microsoft.Extensions.DependencyInjection;
    using Spider.Pipelines.Boundaries;
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;
    using Spider.Pipelines.Flows;
    using Spider.Pipelines.Generated;

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
            await RunComposeFlowExampleAsync();
            await RunComposeFlowBranchExampleAsync();
            await RunComposeFlowWithoutResponseExampleAsync();
            PrintGeneratedArchitectureManifest();
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
                .AddExecutionBoundary<ConsoleBoundary>()
                .Named("Console logging boundary")
                .Describe("Logs the lifecycle of sample order pipeline executions to the console.")
                .BoundaryType("console diagnostic boundary")
                .EntryPoint("Basic sample console runner")
                .Protocol("in-process")
                .Operation("Run sample order pipeline")
                .Contract<OrderRequest, OrderReceipt>()
                .Policies("diagnostic-logging")
                .Observability("console events")
                .InvokesPipeline<OrderRequest, OrderReceipt>();

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
        /// Runs a sample pipeline with delegate callbacks selected through the bridge fluent API.
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
                .AddExecutionBoundary(boundary =>
                {
                    boundary
                        .Named("Fluent delegate boundary")
                        .Describe("Demonstrates callback-based boundary registration on a bridge.")
                        .BoundaryType("delegate boundary")
                        .EntryPoint("Bridge-level sample boundary")
                        .Protocol("in-process")
                        .Contract<OrderRequest, OrderReceipt>()
                        .Policies("demo-callbacks")
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
                        });
                })
                .Attach<OrderRequest, OrderReceipt>(builder => ConfigureOrderPipeline(builder, log))
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
                .Named("Order placement pipeline")
                .Describe("Wraps order placement with validation, logging middleware, read-model notification, and post-processing.")
                .Purpose("Documents the operational wrapper around SampleOrderService.PlaceOrderAsync.")
                .Trigger("Basic console sample")
                .Wraps("SampleOrderService.PlaceOrderAsync")
                .Input(nameof(OrderRequest))
                .Output(nameof(OrderReceipt))
                .Policies("validation", "console-observability")
                .FailureBehavior("Writes the failure to the sample event log.")
                .Module("Order sample")
                .PreProcess((ctx, args) =>
                {
                    log.Write($"preprocess: validating order {ctx.Request.OrderId}");
                    return Task.CompletedTask;
                }, stage => stage
                    .Named("Validate order request")
                    .Describe("Checks the incoming order before the service handler runs.")
                    .Purpose("Stops malformed console sample requests before the order handler executes.")
                    .Policies("validation", "guard")
                    .Observability("order-validation-started")
                    .Timeout("Expected below 5 ms.")
                    .Tags("validation", "pre-process"))
                .UseMiddleware(async (ctx, next) =>
                {
                    log.Write("middleware: before handler");
                    var response = await next();
                    log.Write("middleware: after handler");
                    return response;
                }, stage => stage
                    .Named("Log handler execution")
                    .Describe("Writes before and after messages around the target handler.")
                    .Purpose("Shows how middleware wraps the target service without changing the response contract.")
                    .Wraps("SampleOrderService.PlaceOrderAsync")
                    .Policies("console-observability", "transparent-wrapper")
                    .Observability("handler-before", "handler-after")
                    .Tags("logging", "middleware"))
                .Parallel((ctx, args) =>
                {
                    log.Write("parallel: notifying read model");
                    return Task.CompletedTask;
                }, stage => stage
                    .Named("Notify read model")
                    .Describe("Simulates a side effect that can run beside the main operation.")
                    .Purpose("Documents asynchronous read-model notification beside the primary order operation.")
                    .Policies("non-blocking", "best-effort")
                    .External("Sample read model")
                    .Observability("read-model-notified")
                    .FailureBehavior("Read-model notification failure is diagnostic and does not replace the order response.")
                    .Tags("parallel", "read-model"))
                .OnSuccess((ctx, args) =>
                {
                    log.Write($"postprocess: receipt {ctx.Response.ReceiptId}");
                    return Task.CompletedTask;
                }, stage => stage
                    .Named("Record order receipt")
                    .Describe("Logs the receipt after a successful order placement.")
                    .Purpose("Records the sample receipt when the order pipeline succeeds.")
                    .Policies("success-only", "audit")
                    .Observability("order-receipt-recorded")
                    .Tags("success", "receipt"))
                .OnFailure((ctx, args) =>
                {
                    log.Write($"postprocess: failure {ctx.Exception?.Message}");
                    return Task.CompletedTask;
                }, stage => stage
                    .Named("Record order failure")
                    .Describe("Logs the exception captured during order placement.")
                    .Purpose("Records enough context to understand failed sample order executions.")
                    .Policies("fault-capture", "diagnostic")
                    .Observability("order-failure-recorded")
                    .FailureBehavior("Logs the failure and lets the original exception continue.")
                    .Tags("failure", "diagnostic"));
        }

        /// <summary>
        /// Runs a sample business flow that produces a receipt.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunComposeFlowExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleEventLog>();
            services.AddSpider(builder =>
            {
                builder.AddFlowProfile("Business", profile =>
                {
                    profile.TelemetryEnabled = true;
                    profile.MetricsEnabled = true;
                });
            });

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            log.Write("example: compose flow with response");

            var receipt = await spider
                .ComposeFlow<OrderRequest, OrderReceipt>("Create order receipt")
                .UsingProfile("Business")
                .Then(request =>
                {
                    log.Write($"flow: validate {request.OrderId}");
                    if (request.Total <= 0)
                        throw new InvalidOperationException("Order total must be positive.");
                })
                .Then(MapOrder)
                .ThenWith<OrderRequest, FlowOrder>(async (request, order, token) =>
                {
                    log.Write($"flow: save {request.OrderId} for {order.Total:C}");
                    await Task.CompletedTask;
                })
                .Then(ReturnReceipt)
                .RunAsync(new OrderRequest("SO-2001", 180m));

            log.Write($"flow done: {receipt.ReceiptId} for {receipt.Total:C}");
        }

        /// <summary>
        /// Runs a sample business flow with a conditional branch.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunComposeFlowBranchExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleEventLog>();
            services.AddSpider();

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            log.Write("example: compose flow with branch");

            var receipt = await spider
                .ComposeFlow<OrderRequest, OrderReceipt>("Approve order")
                .Then(MapOrder)
                .Branch<FlowOrderDecision>(branch => branch
                    .When(IsSmallOrder, small => small.Then(order =>
                    {
                        log.Write("flow-branch: auto approve");
                        return new FlowOrderDecision(order, "auto-approved");
                    }))
                    .Otherwise(large => large.Then(order =>
                    {
                        log.Write("flow-branch: manual review");
                        return new FlowOrderDecision(order, "manual-review");
                    })))
                .Then(decision =>
                {
                    log.Write($"flow: decision {decision.Decision}");
                    return new OrderReceipt($"RCPT-{decision.Order.OrderId}", decision.Order.Total);
                })
                .RunAsync(new OrderRequest("SO-2002", 950m));

            log.Write($"branch flow done: {receipt.ReceiptId} for {receipt.Total:C}");
        }

        /// <summary>
        /// Runs a sample business flow that can stop without a response.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunComposeFlowWithoutResponseExampleAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SampleEventLog>();
            services.AddSpider();

            var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();
            var log = provider.GetRequiredService<SampleEventLog>();

            log.Write("example: compose flow without response");

            await spider
                .ComposeFlow<OrderRequest>("Notify order")
                .ContinueIf(ShouldNotifyOrder, Flow.Stop())
                .Then(request =>
                {
                    log.Write($"flow: notify {request.OrderId}");
                })
                .RunAsync(new OrderRequest("SO-2003", 0m));

            log.Write("void flow done");
        }

        /// <summary>
        /// Prints architecture metadata generated at compilation time by the Spider analyzer.
        /// </summary>
        private static void PrintGeneratedArchitectureManifest()
        {
            var manifest = SpiderGeneratedArchitecture.BuildManifest();
            Console.WriteLine($"generated metadata: {manifest.Components.Count} components, {manifest.Relations.Count} relations");

            foreach (var component in manifest.Components.Take(5))
                Console.WriteLine($"generated-component: {component.Kind} {component.Id}");
        }

        /// <summary>
        /// Maps an order request to a flow order.
        /// </summary>
        /// <param name="request">The order request.</param>
        /// <returns>The mapped flow order.</returns>
        private static FlowOrder MapOrder(OrderRequest request)
            => new(request.OrderId, request.Total);

        /// <summary>
        /// Creates an order receipt from a flow order.
        /// </summary>
        /// <param name="order">The flow order.</param>
        /// <returns>The created receipt.</returns>
        private static OrderReceipt ReturnReceipt(FlowOrder order)
            => new($"RCPT-{order.OrderId}", order.Total);

        /// <summary>
        /// Determines whether an order can be auto-approved.
        /// </summary>
        /// <param name="order">The flow order.</param>
        /// <returns><see langword="true"/> when the order can be auto-approved; otherwise, <see langword="false"/>.</returns>
        private static bool IsSmallOrder(FlowOrder order)
            => order.Total < 500m;

        /// <summary>
        /// Determines whether an order notification should be sent.
        /// </summary>
        /// <param name="request">The order request.</param>
        /// <returns><see langword="true"/> when the order should be notified; otherwise, <see langword="false"/>.</returns>
        private static bool ShouldNotifyOrder(OrderRequest request)
            => request.Total > 0m;

        /// <summary>
        /// Represents an internal order used by the flow sample.
        /// </summary>
        /// <param name="OrderId">The order identifier.</param>
        /// <param name="Total">The order total.</param>
        private sealed record FlowOrder(string OrderId, decimal Total);

        /// <summary>
        /// Represents an order decision used by the flow sample.
        /// </summary>
        /// <param name="Order">The evaluated order.</param>
        /// <param name="Decision">The decision value.</param>
        private sealed record FlowOrderDecision(FlowOrder Order, string Decision);
    }
}
