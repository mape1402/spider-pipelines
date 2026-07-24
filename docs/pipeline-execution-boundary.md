# Pipeline Execution Boundary

## Objective

Spider provides typed execution boundaries that wrap a complete pipeline execution. Boundaries can be registered globally, selected per fluent pipeline configuration, or passed to a single invocation.

## Boundary Contract

Request-only boundaries implement `IPipelineExecutionBoundary<TRequest>`:

```csharp
public interface IPipelineExecutionBoundary<TRequest>
{
    ValueTask BeginAsync(IReadOnlyContext<TRequest> context, CancellationToken cancellationToken);

    ValueTask CompleteAsync(IReadOnlyContext<TRequest> context, CancellationToken cancellationToken);

    ValueTask FaultAsync(IReadOnlyContext<TRequest> context, Exception exception, CancellationToken cancellationToken);

    ValueTask CancelAsync(IReadOnlyContext<TRequest> context, CancellationToken cancellationToken);

    ValueTask DisposeAsync(IReadOnlyContext<TRequest> context, CancellationToken cancellationToken);
}
```

Request/response boundaries implement `IPipelineExecutionBoundary<TRequest, TResponse>`:

```csharp
public interface IPipelineExecutionBoundary<TRequest, TResponse>
{
    ValueTask BeginAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);

    ValueTask CompleteAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);

    ValueTask FaultAsync(IReadOnlyContext<TRequest, TResponse> context, Exception exception, CancellationToken cancellationToken);

    ValueTask CancelAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);

    ValueTask DisposeAsync(IReadOnlyContext<TRequest, TResponse> context, CancellationToken cancellationToken);
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
    spider.AddExecutionBoundary<OrderRequest, OrderReceipt, OrderBoundary>();
});
```

For request-only pipelines:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary<OrderRequest, OrderBoundary>();
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

Pass typed boundary instances to a single `ExecuteAsync` call:

```csharp
await bridge.ExecuteAsync(
    service => (request, token) => service.HandleAsync(request, token),
    request,
    new[] { myBoundary });
```

## Context

Boundary callbacks receive the same typed pipeline context shape used by pipeline stages:

```csharp
IReadOnlyContext<TRequest>
IReadOnlyContext<TRequest, TResponse>
```

That gives boundaries typed access to `Request`, `Response`, `Services`, cancellation state, pipeline state, result state, and captured exceptions.
