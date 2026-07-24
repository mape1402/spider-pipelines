# Spider.Pipelines

**Modular, flexible operation pipelines for .NET.**

[![Build](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/Spider.Pipelines.svg)](https://www.nuget.org/packages/Spider.Pipelines/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Spider.Pipelines is a lightweight .NET library for composing service execution pipelines. It lets you attach preprocessors, middleware, override handlers, parallel steps, and postprocessors around existing logic with a clean, dependency-injection-friendly API.

## Features

- Modular pipeline stages for preprocessing, middleware, targeting, parallel work, and postprocessing.
- Delegate-based execution with minimal runtime overhead.
- Dependency-injection friendly registration through `IServiceCollection`.
- Immutable pipeline step snapshots at execution build time.
- Thread-safe context state for concurrent target and parallel stages.
- Provider-agnostic execution boundaries for wrapping complete pipeline execution.
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

Execution boundaries can be registered globally through the Spider builder:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary<MyBoundary>();
});
```

Global boundaries wrap every Spider pipeline execution resolved from that container.

They can also be selected for a single attached pipeline through the fluent API. Register the implementation as a normal service, then add it to the pipeline:

```csharp
services.AddScoped<MyBoundary>();

bridge.Attach<string, string>(builder =>
{
    builder.AddExecutionBoundary<MyBoundary>();
});
```

Or configure the boundary callbacks inline:

```csharp
bridge.Attach<string, string>(builder =>
{
    builder.AddExecutionBoundary(boundary =>
    {
        boundary.OnBegin((ctx, token) => ValueTask.CompletedTask);
        boundary.OnComplete((ctx, token) => ValueTask.CompletedTask);
        boundary.OnFault((ctx, ex, token) => ValueTask.CompletedTask);
        boundary.OnCancel((ctx, token) => ValueTask.CompletedTask);
        boundary.OnDispose(ctx => ValueTask.CompletedTask);
    });
});
```

For a single execution, configure DI-resolved invocation boundaries directly on `ExecuteAsync`:

```csharp
await typedBridge.ExecuteAsync(
    svc => (input, token) => svc.Handle(input, token),
    "World",
    execution => execution.AddExecutionBoundary<MyBoundary>());
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

Boundaries wrap the full pipeline execution and stay provider-agnostic. Spider resolves typed boundaries from DI and calls `BeginAsync`, then exactly one terminal operation: `CompleteAsync`, `FaultAsync`, or `CancelAsync`, followed by `DisposeAsync`.

```csharp
public sealed class MyBoundary<TRequest, TResponse> : IBoundary<TRequest, TResponse>
{
    public ValueTask BeginAsync(
        PipelineExecutionContext<TRequest, TResponse> context,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask CompleteAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask FaultAsync(PipelineExecutionContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask CancelAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
```

For request-only pipelines, implement `IBoundary<TRequest>`:

```csharp
public sealed class MyBoundary<TRequest> : IBoundary<TRequest>
{
    public ValueTask BeginAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask CompleteAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask FaultAsync(PipelineExecutionContext<TRequest> context, Exception exception, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask CancelAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
```

Boundary order:

1. Boundary begin.
2. Preprocessors.
3. Middleware, target/override, and parallel work.
4. Postprocessors.
5. Boundary complete, fault, or cancel.
6. Boundary dispose.

### Global Boundaries

Global boundaries are registered once and wrap every pipeline execution from the same container:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary<AuditBoundary>();
    spider.AddExecutionBoundary<TransactionBoundary>();
});
```

Reusable open-generic boundaries can be registered explicitly by type:

```csharp
services.AddSpider(spider =>
{
    spider.AddBoundary(typeof(MyBoundary<,>));
});
```

### Fluent API Boundaries

Fluent boundaries are part of one attached pipeline. Register the implementation as a normal DI service, then select it from the builder:

```csharp
services.AddScoped<OrderBoundary>();
services.AddSpider();

var bridge = spider
    .InitBridge<OrderService>()
    .Attach<OrderRequest, OrderReceipt>(builder =>
    {
        builder
            .AddExecutionBoundary<OrderBoundary>()
            .PreProcess((ctx, args) => Task.CompletedTask)
            .OnSuccess((ctx, args) => Task.CompletedTask);
    });
```

Fluent boundaries can also be defined with callbacks instead of a class:

```csharp
var bridge = spider
    .InitBridge<OrderService>()
    .Attach<OrderRequest, OrderReceipt>(builder =>
    {
        builder.AddExecutionBoundary(boundary =>
        {
            boundary.OnBegin((ctx, token) => ValueTask.CompletedTask);
            boundary.OnComplete((ctx, token) => ValueTask.CompletedTask);
            boundary.OnFault((ctx, ex, token) => ValueTask.CompletedTask);
            boundary.OnCancel((ctx, token) => ValueTask.CompletedTask);
            boundary.OnDispose(ctx => ValueTask.CompletedTask);
        });
    });
```

Open-generic boundaries can also be selected for one attached pipeline by runtime type, as long as the implementation is registered in DI:

```csharp
services.AddScoped(typeof(MyBoundary<,>));

var bridge = spider
    .InitBridge<OrderService>()
    .Attach<OrderRequest, OrderReceipt>(builder =>
    {
        builder.AddBoundary(typeof(MyBoundary<,>));
    });
```

### Invocation Boundaries

Invocation boundaries apply only to one `ExecuteAsync` call:

```csharp
await typedBridge.ExecuteAsync(
    svc => (input, token) => svc.Handle(input, token),
    "World",
    execution => execution.AddExecutionBoundary<MyBoundary>());
```

Open-generic boundary types can be selected for one execution too:

```csharp
await typedBridge.ExecuteAsync(
    svc => (input, token) => svc.Handle(input, token),
    "World",
    execution => execution.AddBoundary(typeof(MyBoundary<,>)));
```

They can also be configured inline with callbacks:

```csharp
await typedBridge.ExecuteAsync(
    svc => (input, token) => svc.Handle(input, token),
    "World",
    execution => execution.AddExecutionBoundary(boundary =>
    {
        boundary.OnBegin((ctx, token) => ValueTask.CompletedTask);
        boundary.OnComplete((ctx, token) => ValueTask.CompletedTask);
        boundary.OnFault((ctx, ex, token) => ValueTask.CompletedTask);
        boundary.OnCancel((ctx, token) => ValueTask.CompletedTask);
        boundary.OnDispose(ctx => ValueTask.CompletedTask);
    }));
```

Multiple boundaries begin in this order: global DI, fluent pipeline, invocation. They terminate in reverse order.

## Error and Cancellation Behavior

Target, middleware, and parallel exceptions are captured in the context as `ResultState.Failure`. Failure postprocessors run before the original exception is rethrown by the pipeline.

Calling `ctx.CancelOperation()` sets `ResultState.Cancelled`. Cancellation skips target execution when observed before targeting, prevents success/failure postprocessors from running, and terminates registered boundaries through `CancelAsync`.
