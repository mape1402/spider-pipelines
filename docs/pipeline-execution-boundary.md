# Pipeline Execution Boundary

## Objective

Spider provides a provider-agnostic execution boundary that wraps a complete pipeline execution. The core library owns only the orchestration point; application code or optional integration packages decide what each boundary operation means.

## Boundary Contract

Boundaries implement `IPipelineExecutionBoundary`:

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

Use the Spider builder to register boundaries:

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
