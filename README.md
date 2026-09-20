# Spider.Pipelines

**Modular, flexible operation pipelines for .NET.**

[![Build](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/Spider.Pipelines.svg)](https://www.nuget.org/packages/Spider.Pipelines/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Spider.Pipelines is a lightweight .NET library for composing service execution pipelines. It lets you attach preprocessors, middleware, override handlers, parallel steps, and postprocessors around existing logic with a clean, dependency-injection-friendly API.

Version 2.1.0 targets .NET 8, .NET 9, and .NET 10.

## Features

- Modular pipeline stages for preprocessing, middleware, targeting, parallel work, and postprocessing.
- Delegate-based execution with minimal runtime overhead.
- Dependency-injection friendly registration through `IServiceCollection`.
- Immutable pipeline step snapshots at execution build time.
- Thread-safe context state for concurrent target and parallel stages.
- Provider-agnostic execution boundaries for wrapping complete pipeline execution.
- Method-level `ComposeFlow` for describing local business processes with `Then`, `ThenWith`, `ContinueIf`, and `Branch`.
- Compile-time architecture metadata manifest generated from pipelines and composed flows.
- Graphical web documentation renderer for architecture manifests through `Spider.Pipelines.Web`.
- Testing helpers for boundary traces, execution ordering, transaction assertions, and failure simulation.
- .NET 8, .NET 9, and .NET 10 support.
- Tested with xUnit and NSubstitute.

## Installation

```bash
dotnet add package Spider.Pipelines
```

For testing helpers:

```bash
dotnet add package Spider.Testing
```

For graphical architecture documentation:

```bash
dotnet add package Spider.Pipelines.Web
```

## Samples

Run the basic sample with:

```bash
dotnet run --project samples/Spider.Pipelines.Samples.Basic/Spider.Pipelines.Samples.Basic.csproj
```

Run the web documentation sample with:

```bash
dotnet run --project samples/Spider.Pipelines.Samples.Web/Spider.Pipelines.Samples.Web.csproj
```

Open `http://localhost:5000` or the URL printed by ASP.NET Core.

## ComposeFlow

Pipelines wrap execution with cross-cutting behavior. `ComposeFlow` describes local business steps inside a handler, service, endpoint, or job.

```csharp
var receipt = await spider
    .ComposeFlow<OrderRequest, OrderReceipt>("Create order receipt")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .ThenWith<OrderRequest, Order>(Save)
    .Then(ReturnReceipt)
    .RunAsync(request, cancellationToken);
```

Flows can also run without a response:

```csharp
await spider
    .ComposeFlow<OrderRequest>("Notify order")
    .ContinueIf(ShouldNotifyOrder, Flow.Stop())
    .Then(SendNotification)
    .RunAsync(request, cancellationToken);
```

Use `Branch` when the process needs controlled decision paths:

```csharp
var receipt = await spider
    .ComposeFlow<OrderRequest, OrderReceipt>("Approve order")
    .Then(Map)
    .Branch<OrderDecision>(branch => branch
        .When(IsSmallOrder, small => small.Then(AutoApprove))
        .Otherwise(large => large.Then(RequireManualReview)))
    .Then(ReturnReceipt)
    .RunAsync(request, cancellationToken);
```

## Architecture Metadata

Spider includes a source generator that reads `ComposeFlow` and pipeline `Attach` fluent chains during compilation. It generates a typed manifest without executing the application.

```csharp
using Spider.Pipelines.Generated;

var manifest = SpiderGeneratedArchitecture.BuildManifest();
```

The generated manifest includes:

- `spider.pipeline` components for attached pipelines.
- `spider.pipeline-stage` components for preprocess, middleware, target, parallel, success postprocess, and failure postprocess stages.
- `spider.flow` components for method-level flows.
- `spider.flow-step`, `spider.flow-condition`, and `spider.flow-branch` components for flow steps.
- `spider.flow-profile` components for profiles selected with `UsingProfile`.
- `contains`, `next`, and `uses-profile` relations.
- source evidence with file, line, containing type, and member name when the compiler can resolve it.

This metadata describes the configured architecture and pseudocode-level process map at build time. Runtime execution stays focused on running pipelines and flows.

See [docs/compose-flow-design.md](docs/compose-flow-design.md) for the broader design direction behind flows and metadata generation.

## Web Documentation

`Spider.Pipelines.Web` renders a graphical architecture page from a `SpiderArchitectureManifest`. The package is UI-only: it does not scan the application at runtime and it does not require Minimal API endpoints. Host applications can serve the generated HTML through middleware, MVC, Razor Pages, static file generation, or a future REST adapter.

```csharp
using Spider.Pipelines.Generated;
using Spider.Pipelines.Web;

var manifest = SpiderGeneratedArchitecture.BuildManifest();
var renderer = new SpiderArchitectureWebRenderer();
var html = renderer.Render(manifest, new SpiderArchitectureWebOptions
{
    Title = "Service Architecture"
});
```

The rendered UI includes an SVG graph, flow and pipeline views, source evidence, filters, search, details, and raw JSON inspection. The web sample shows the simplest middleware-based host.

## Testing Package

`Spider.Testing` provides a small test host for executing requests through discovered handlers and boundaries without hand-writing mocks for every pipeline dependency.

Register the package from one or more assemblies:

```csharp
services.AddSpiderTesting(typeof(CustomerBoundary).Assembly);
```

Execute a request and inspect the trace:

```csharp
var spider = provider.GetRequiredService<ISpiderTesting>();

var result = await spider.ExecuteAsync(command);
var trace = spider.Trace;
```

Assert boundary presence and execution order:

```csharp
trace.ShouldContain("TransactionBoundary");
trace.ShouldContain("ExceptionBoundary");
trace.ShouldRunBefore("TransactionBoundary", "HandlerExecution");
```

Assert transaction behavior:

```csharp
trace.Transaction.ShouldBegin();
trace.Transaction.ShouldCommit();
```

Simulate failures inside a specific boundary:

```csharp
spider.FailInside<SomeBoundary>(new InvalidOperationException());
```

## Quick Start

### 1. Register Spider

```csharp
services.AddSpider();
```

Execution boundaries can be registered through the Spider builder:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary<MyBoundary>();
});
```

They can also be selected for a bridge execution flow through the fluent API. Register the implementation as a normal service, then add it after bridge initialization and before attaching the pipeline:

```csharp
services.AddScoped<MyBoundary>();

var typedBridge = spider
    .InitBridge<MyService>()
    .AddExecutionBoundary<MyBoundary>()
    .Attach<string, string>(builder => { });
```

Inline boundary callbacks can be configured directly on the bridge:

```csharp
var typedBridge = spider
    .InitBridge<MyService>()
    .AddExecutionBoundary(boundary =>
    {
        boundary.OnBegin((ctx, token) => ValueTask.CompletedTask);
        boundary.OnComplete((ctx, token) => ValueTask.CompletedTask);
        boundary.OnFault((ctx, ex, token) => ValueTask.CompletedTask);
        boundary.OnCancel((ctx, token) => ValueTask.CompletedTask);
    })
    .Attach<string, string>(builder => { });
```

### 2. Define a Service

```csharp
public class MyService
{
    public Task<string> Handle(string input, CancellationToken token)
    {
        return Task.FromResult($"Hello, {input}!");
    }
}
```

### 3. Attach Pipeline Steps

```csharp
var spider = provider.GetRequiredService<ISpider>();

var bridge = spider.InitBridge<MyService>();
var typedBridge = bridge.Attach<string, string>(builder =>
{
    builder
        .PreProcess((ctx, args) =>
        {
            Console.WriteLine($"Preprocessing: {ctx.Request}");
            return Task.CompletedTask;
        })
        .UseMiddleware(async (ctx, next) =>
        {
            Console.WriteLine("Before target");
            var response = await next();
            Console.WriteLine("After target");
            return response;
        })
        .UseOverride((req, token) => Task.FromResult($"Targeted: {req}"))
        .Parallel((ctx, args) =>
        {
            Console.WriteLine($"Parallel work for: {ctx.Request}");
            return Task.CompletedTask;
        })
        .OnSuccess((ctx, args) =>
        {
            Console.WriteLine($"Success: {ctx.Response}");
            return Task.CompletedTask;
        });
});
```

### 4. Execute the Pipeline

```csharp
var result = await typedBridge.ExecuteAsync(
    svc => (input, token) => svc.Handle(input, token),
    "World"
);

Console.WriteLine(result);
```

## Execution Contract

The default order is:

1. Preprocessors run in registration order.
2. Middleware wraps the target handler.
3. Targeting runs the override handler when configured, otherwise the service handler.
4. Parallel steps run concurrently with middleware and target execution.
5. Success or failure postprocessors run after targeting and parallel work complete.

Parallel steps are for work that should truly run at the same time as the main operation. Use preprocessors for before-target work, postprocessors for after-target work, and middleware when you need to wrap the target.

Context state is synchronized while target and parallel steps run concurrently, but user-provided request/response objects should still be treated with normal .NET thread-safety rules.

## Execution Boundaries

Boundaries wrap the full pipeline execution and stay provider-agnostic. Spider resolves the boundary from DI and calls `BeginAsync`, then exactly one terminal operation: `CompleteAsync`, `FaultAsync`, or `CancelAsync`.

Use `PipelineExecutionBoundary` when a boundary only needs some callbacks. It provides no-op defaults for every operation:

```csharp
public sealed class MyBoundary : PipelineExecutionBoundary
{
    public override ValueTask BeginAsync(
        PipelineExecutionContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public override ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
```

Implement `IPipelineExecutionBoundary` directly when a boundary intentionally owns all four operations.

Boundary order:

1. Boundary begin.
2. Preprocessors.
3. Middleware, target/override, and parallel work.
4. Postprocessors.
5. Boundary complete, fault, or cancel.

For a bridge execution flow, select DI-registered boundaries after initializing the bridge and before attaching the pipeline:

```csharp
var typedBridge = spider
    .InitBridge<MyService>()
    .AddExecutionBoundary<MyBoundary>()
    .AddExecutionBoundary<OtherBoundary>()
    .Attach<string, string>(builder => { });

await typedBridge.ExecuteAsync(
    svc => (input, token) => svc.Handle(input, token),
    "World");
```

Multiple boundaries begin in this order: global DI, bridge-selected. They terminate in reverse order.

## Error and Cancellation Behavior

Target, middleware, and parallel exceptions are captured in the context as `ResultState.Failure`. Failure postprocessors run before the original exception is rethrown by the pipeline.

Calling `ctx.CancelOperation()` sets `ResultState.Cancelled`. Cancellation skips target execution when observed before targeting, prevents success/failure postprocessors from running, and terminates registered boundaries through `CancelAsync`.
