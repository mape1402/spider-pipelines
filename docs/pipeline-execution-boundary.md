# Pipeline Execution Boundary

## Objective

Spider provides typed execution boundaries that wrap a complete pipeline execution. Boundaries can be registered globally, selected per fluent pipeline configuration, or configured for a single invocation.

## Boundary Contract

Request-only boundaries implement `IBoundary<TRequest>`:

```csharp
public interface IBoundary<TRequest>
{
    ValueTask BeginAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken);

    ValueTask CompleteAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken);

    ValueTask FaultAsync(PipelineExecutionContext<TRequest> context, Exception exception, CancellationToken cancellationToken);

    ValueTask CancelAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken);

    ValueTask DisposeAsync(PipelineExecutionContext<TRequest> context, CancellationToken cancellationToken);
}
```

For reusable request-only infrastructure, implement the boundary as a generic type:

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

Request/response boundaries implement `IBoundary<TRequest, TResponse>`:

```csharp
public interface IBoundary<TRequest, TResponse>
{
    ValueTask BeginAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken);

    ValueTask CompleteAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken);

    ValueTask FaultAsync(PipelineExecutionContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken);

    ValueTask CancelAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken);

    ValueTask DisposeAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken);
}
```

For reusable request/response infrastructure, implement the boundary as a generic type:

```csharp
public sealed class MyBoundary<TRequest, TResponse> : IBoundary<TRequest, TResponse>
{
    public ValueTask BeginAsync(PipelineExecutionContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

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

## Execution Order

For a successful pipeline:

```txt
Boundary.Begin
  Pre-processors
  Middleware / Target / Parallel work
  Post-processors
Boundary.Complete
Boundary.Dispose
```

For a faulted pipeline:

```txt
Boundary.Begin
  Pipeline throws
Boundary.Fault
Boundary.Dispose
```

For a cooperatively cancelled pipeline:

```txt
Boundary.Begin
  Pipeline cancels
Boundary.Cancel
Boundary.Dispose
```

## Multiple Boundaries

Boundaries begin in this order:

```txt
Global DI boundaries
Pipeline fluent boundaries
Invocation boundaries
```

Terminal and dispose callbacks run in reverse order.

## Exception Rules

- If the pipeline throws, Spider calls `FaultAsync` and rethrows the original exception.
- If the pipeline is cancelled through `CancelOperation`, Spider calls `CancelAsync`.
- If `BeginAsync` fails, already opened boundaries are faulted and disposed.
- If `CompleteAsync`, `FaultAsync`, `CancelAsync`, or `DisposeAsync` fails after the pipeline already threw, the original pipeline exception is preserved.
- If the pipeline succeeded and `CompleteAsync` or `DisposeAsync` fails, the boundary exception is surfaced.

## Global Registration

Register a global request/response boundary with the Spider builder:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary<OrderBoundary>();
});
```

For request-only pipelines, the same registration shape applies:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary<OrderBoundary>();
});
```

Spider discovers whether `OrderBoundary` implements `IBoundary<TRequest>` or `IBoundary<TRequest, TResponse>` and registers it against that typed contract. Reusable open-generic boundaries can be registered explicitly by type:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary(typeof(OrderBoundary<,>));
});
```

## Fluent API

Select a DI-registered boundary for one attached pipeline:

```csharp
services.AddScoped<OrderBoundary>();

spider.InitBridge<OrderService>()
    .Attach<OrderRequest, OrderReceipt>(builder =>
    {
        builder.AddExecutionBoundary<OrderBoundary>();
    });
```

Configure a delegate-backed boundary directly in the fluent pipeline:

```csharp
builder.AddExecutionBoundary(boundary =>
{
    boundary.OnBegin((ctx, token) => ValueTask.CompletedTask);
    boundary.OnComplete((ctx, token) => ValueTask.CompletedTask);
    boundary.OnFault((ctx, ex, token) => ValueTask.CompletedTask);
    boundary.OnCancel((ctx, token) => ValueTask.CompletedTask);
    boundary.OnDispose(ctx => ValueTask.CompletedTask);
});
```

You can also provide an already-created typed boundary instance:

```csharp
builder.AddExecutionBoundary(myBoundary);
```

## Invocation Boundaries

Configure typed boundaries for a single `ExecuteAsync` call:

```csharp
await bridge.ExecuteAsync(
    service => (request, token) => service.HandleAsync(request, token),
    request,
    execution => execution.AddExecutionBoundary(myBoundary));
```

Invocation boundaries can also be resolved from DI:

```csharp
await bridge.ExecuteAsync(
    service => (request, token) => service.HandleAsync(request, token),
    request,
    execution => execution.AddExecutionBoundary<OrderBoundary>());
```

Or configured inline with callbacks:

```csharp
await bridge.ExecuteAsync(
    service => (request, token) => service.HandleAsync(request, token),
    request,
    execution => execution.AddExecutionBoundary(boundary =>
    {
        boundary.OnBegin((ctx, token) => ValueTask.CompletedTask);
        boundary.OnComplete((ctx, token) => ValueTask.CompletedTask);
        boundary.OnFault((ctx, ex, token) => ValueTask.CompletedTask);
        boundary.OnCancel((ctx, token) => ValueTask.CompletedTask);
        boundary.OnDispose(ctx => ValueTask.CompletedTask);
    }));
```

## Context

Boundary callbacks receive typed, provider-agnostic boundary contexts:

```csharp
PipelineExecutionContext<TRequest>
PipelineExecutionContext<TRequest, TResponse>
```

That gives boundaries typed access to `Request`, `Response` for request/response pipelines, `Services`, and per-execution `Items` without exposing Spider's internal pipeline context.
