# Spider.Pipelines

**Modular, flexible operation pipelines for .NET.**

[![Build](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/Spider.Pipelines.svg)](https://www.nuget.org/packages/Spider.Pipelines/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Spider.Pipelines is a lightweight .NET library for composing service execution pipelines. It lets you attach preprocessors, middleware, override handlers, parallel steps, and postprocessors around existing logic with a clean, dependency-injection-friendly API.

Version 2.0.0 targets .NET 8, .NET 9, and .NET 10.

## Features

- Modular pipeline stages for preprocessing, middleware, targeting, parallel work, and postprocessing.
- Delegate-based execution with minimal runtime overhead.
- Dependency-injection friendly registration through `IServiceCollection`.
- Immutable pipeline step snapshots at execution build time.
- Thread-safe context state for concurrent target and parallel stages.
- Provider-agnostic execution boundaries for wrapping complete pipeline execution.
- .NET 8, .NET 9, and .NET 10 support.
- Tested with xUnit and NSubstitute.

## Installation

```bash
dotnet add package Spider.Pipelines
```

## Samples

Run the basic sample with:

```bash
dotnet run --project samples/Spider.Pipelines.Samples.Basic/Spider.Pipelines.Samples.Basic.csproj
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
