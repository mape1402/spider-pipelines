# Spider.Pipelines

**Modular, flexible operation pipelines for .NET.**

[![Build](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/spider-pipelines/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/Spider.Pipelines.svg)](https://www.nuget.org/packages/Spider.Pipelines/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Spider.Pipelines is a lightweight .NET library for composing service execution pipelines. It lets you attach preprocessors, override handlers, parallel steps, and postprocessors around existing logic with a clean, dependency-injection-friendly API.

## Features

- Modular pipeline stages for preprocessing, targeting, parallel work, and postprocessing.
- Delegate-based execution with minimal runtime overhead.
- Dependency-injection friendly registration through `IServiceCollection`.
- Immutable pipeline step snapshots at execution build time.
- Thread-safe context state for concurrent target and parallel stages.
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
    builder.OnPreProcess(cfg =>
    {
        cfg.OnPreProcess((ctx, args) =>
        {
            Console.WriteLine($"Preprocessing: {ctx.Request}");
            return Task.CompletedTask;
        });
    });

    builder.OnTargeting(cfg =>
    {
        cfg.Overrides((req, token) => Task.FromResult($"Targeted: {req}"));
    });

    builder.OnPostProcess(cfg =>
    {
        cfg.OnSuccess((ctx, args) =>
        {
            Console.WriteLine($"Success: {ctx.Response}");
            return Task.CompletedTask;
        });
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

## Upcoming Features

- Middleware execution.
