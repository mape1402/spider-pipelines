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
- Explicit parallel execution modes: before, with, or after the target handler.
- Tested with xUnit and NSubstitute.

## Installation

```bash
dotnet add package Spider.Pipelines
```

## Quick Start

### 1. Register Spider

```csharp
services.AddSpider();
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
        .ParallelMode(ParallelExecutionMode.AfterTarget)
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
4. Parallel steps run according to `ParallelExecutionMode`. The default is `WithTarget` for backwards compatibility.
5. Success or failure postprocessors run after targeting and parallel work complete.

`ParallelExecutionMode.BeforeTarget` runs parallel steps before middleware and target. If a parallel step fails, the target is skipped and failure postprocessors run.

`ParallelExecutionMode.WithTarget` runs parallel steps concurrently with middleware and target. Context state is synchronized, but user-provided request/response objects should still be treated with normal .NET thread-safety rules.

`ParallelExecutionMode.AfterTarget` runs parallel steps after a successful target. If the target fails or the operation is cancelled, after-target parallel steps are skipped.

## Error and Cancellation Behavior

Target, middleware, and parallel exceptions are captured in the context as `ResultState.Failure`. Failure postprocessors run before the original exception is rethrown by the pipeline.

Calling `ctx.CancelOperation()` sets `ResultState.Cancelled`. Cancellation skips target execution when observed before targeting and prevents success/failure postprocessors from running.
