# 🕷️Spider.Pipelines

**Modular. Flexible. Seamless operation pipelines for .NET.**

[![Build](https://github.com/mape1402/pelican/actions/workflows/publish.yaml/badge.svg)](https://github.com/mape1402/pelican/actions/workflows/publish.yaml)
[![NuGet](https://img.shields.io/nuget/v/Spider.Pipelines.svg)](https://www.nuget.org/packages/Spider.Pipelines/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

---

**Spider.Pipelines** is a lightweight and flexible .NET library designed to help developers build powerful operation pipelines. It lets you easily attach preprocessors, middlewares, override methods, and postprocessors to your existing logic — just like a spider weaving a smart, modular web. With its clean API and pluggable architecture, you can extend, intercept, or transform operations step by step, making your code more maintainable and testable.

---

- ## ✨ Features

  - 🕷️ Modular: build pipelines with preprocessors, middlewares, overrides, and postprocessors
  - ⚡ Fast: delegate-based and optimized for performance
  - 🔌 DI-friendly: plug seamlessly into any `IServiceProvider`
  - 🧩 Composable: chain and nest pipeline steps with full control
  - 🧼 Zero dependencies (except for DI abstractions)
  - 🧪 Battle-tested with xUnit & NSubstitute
  - 🛠️ Extensible: create custom steps, handlers, and behaviors
  - 🧠 Introspectable: pipeline metadata available at runtime
  - 🧵 Thread-safe: pipelines are designed for concurrent execution

---

## 📦 Installation

```bash
dotnet add package Spider.Pipelines
```

## 🚀 Quick Start

### 1. Register Spider in your DI container
```c#
services.AddSpider();
```

### 2. Define a service and pipeline
```c#
public class MyService
{
    public Task<string> Handle(string input, CancellationToken token)
    {
        // Your business logic here
        return Task.FromResult($"Hello, {input}!");
    }
}
```

### 3. Attach pipeline steps
```c#
var spider = provider.GetRequiredService<ISpider>();

var bridge = spider.InitBridge<MyService>();
bridge.Attach<string, string>(builder =>
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

### 4. Execute the pipeline
```C#
var result = await bridge.ExecuteAsync(
    svc => (input, token) => svc.Handle(input, token),
    "World"
);
Console.WriteLine(result); // Output: Hello, World!
```

## 🛠️ Upcoming Features

- **Add middlewares**
   Adding middleware execution.
