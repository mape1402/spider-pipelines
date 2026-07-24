# Pipeline Execution Boundary

## Objective

Spider provides a provider-agnostic execution boundary that wraps a complete pipeline execution. The core library owns only the orchestration point; application code or optional integration packages decide what each boundary operation means.

## Boundary Contract

Boundaries can implement `IPipelineExecutionBoundary` directly:

```csharp
public interface IPipelineExecutionBoundary
{
    ValueTask BeginAsync(
        PipelineExecutionContext context,
        CancellationToken cancellationToken);

    ValueTask CompleteAsync(
        PipelineExecutionContext context,
        CancellationToken cancellationToken);

    ValueTask FaultAsync(
        PipelineExecutionContext context,
        Exception exception,
        CancellationToken cancellationToken);

    ValueTask CancelAsync(
        PipelineExecutionContext context,
        CancellationToken cancellationToken);
}
```

Most custom boundaries should inherit `PipelineExecutionBoundary` instead. It implements the interface with no-op defaults so a boundary can override only the callbacks it needs:

```csharp
public sealed class MyBoundary : PipelineExecutionBoundary
{
    public override ValueTask BeginAsync(
        PipelineExecutionContext context,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public override ValueTask FaultAsync(
        PipelineExecutionContext context,
        Exception exception,
        CancellationToken cancellationToken)
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
```

For a faulted pipeline:

```txt
Boundary.Begin
  Pipeline throws
Boundary.Fault
```

For a cooperatively cancelled pipeline:

```txt
Boundary.Begin
  Pipeline cancels
Boundary.Cancel
```

## Multiple Boundaries

Boundaries begin in registration order and terminate in reverse order:

```txt
BoundaryA.Begin
  BoundaryB.Begin
    Pipeline
  BoundaryB.Complete
BoundaryA.Complete
```

## Exception Rules

- If the pipeline throws, Spider calls `FaultAsync` and rethrows the original exception.
- If the pipeline is cancelled through `CancelOperation`, Spider calls `CancelAsync`.
- If `BeginAsync` fails, already opened boundaries are faulted.
- If `CompleteAsync`, `FaultAsync`, or `CancelAsync` fails after the pipeline already threw, the original pipeline exception is preserved.
- If the pipeline succeeded and `CompleteAsync` fails, the boundary exception is surfaced.

## Registration

Register global boundaries with the Spider builder:

```csharp
services.AddSpider(spider =>
{
    spider.AddExecutionBoundary<MyBoundary>();
});
```

Or chain from the returned builder:

```csharp
services
    .AddSpider()
    .AddExecutionBoundary<MyBoundary>();
```

Configure boundaries for a bridge execution flow with the bridge fluent API. The boundary implementation must be registered as a normal DI service; this fluent call only selects it for executions created from that bridge:

```csharp
services.AddScoped<MyBoundary>();

var bridge = spider
    .InitBridge<MyService>()
    .AddExecutionBoundary<MyBoundary>()
    .Attach<MyRequest, MyResponse>(builder => { });
```

You can also select a DI-registered boundary by runtime type through the bridge fluent API:

```csharp
var bridge = spider
    .InitBridge<MyService>()
    .AddExecutionBoundary(typeof(MyBoundary))
    .Attach<MyRequest, MyResponse>(builder => { });
```

Configure inline boundary callbacks through the bridge fluent API when no reusable implementation is needed:

```csharp
var bridge = spider
    .InitBridge<MyService>()
    .AddExecutionBoundary(boundary =>
    {
        boundary.OnBegin((ctx, token) => ValueTask.CompletedTask);
        boundary.OnComplete((ctx, token) => ValueTask.CompletedTask);
        boundary.OnFault((ctx, ex, token) => ValueTask.CompletedTask);
        boundary.OnCancel((ctx, token) => ValueTask.CompletedTask);
    })
    .Attach<MyRequest, MyResponse>(builder => { });
```

For a bridge execution flow, select DI-registered boundaries after initializing the bridge and before attaching the pipeline:

```csharp
var bridge = spider
    .InitBridge<MyService>()
    .AddExecutionBoundary<MyBoundary>()
    .AddExecutionBoundary(typeof(OtherBoundary))
    .Attach<MyRequest, MyResponse>(builder => { });

await bridge.ExecuteAsync(
    service => (request, token) => service.HandleAsync(request, token),
    request);
```

When boundaries are provided from multiple levels, Spider begins them in this order:

```txt
Global DI boundaries
Bridge-selected boundaries
```

Termination still runs in reverse order.

## Context

`PipelineExecutionContext` exposes Spider-owned metadata:

```csharp
public sealed class PipelineExecutionContext
{
    public Type RequestType { get; init; }

    public Type ResponseType { get; init; }

    public object Request { get; init; }

    public IServiceProvider Services { get; init; }

    public IDictionary<string, object> Items { get; }
}
```

Boundary implementations may use this metadata to decide whether and how to activate, without Spider taking a dependency on any concrete infrastructure policy.
