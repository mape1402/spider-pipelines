# Spider Testing

## Objective

`Spider.Testing` provides test helpers for boundaries, request execution, ordering, transaction behavior, request metadata, and failure simulation without hand-writing mocks for every pipeline dependency.

The package does not depend on TurtlePath.

## Registration

Register Spider testing from one or more assemblies:

```csharp
services.AddSpiderTesting(typeof(CustomerBoundary).Assembly);
```

The package discovers:

- concrete `IPipelineExecutionBoundary` implementations
- public `Handle` and `HandleAsync` methods

Handlers may accept:

```csharp
HandleAsync(TRequest request)
HandleAsync(TRequest request, CancellationToken cancellationToken)
Handle(TRequest request)
Handle(TRequest request, CancellationToken cancellationToken)
```

Supported return shapes:

- `void`
- `Task`
- `Task<TResponse>`
- `ValueTask`
- `ValueTask<TResponse>`
- `TResponse`

## Execution

Resolve the testing host and execute a request:

```csharp
var spider = provider.GetRequiredService<ISpiderTesting>();

var result = await spider.ExecuteAsync(command);
```

For typed results:

```csharp
var result = await spider.ExecuteAsync<OrderReceipt>(command);
```

## Trace

The latest execution trace is exposed through:

```csharp
var trace = spider.Trace;
```

The trace records:

- boundary begin, complete, fault, cancel, commit, and rollback operations
- handler execution
- pipeline execution
- request metadata
- exceptions

## Assertions

Boundary presence:

```csharp
trace.ShouldContain("TransactionBoundary");
trace.ShouldContain("ExceptionBoundary");
```

Ordering:

```csharp
trace.ShouldRunBefore("TransactionBoundary", "HandlerExecution");
```

Request metadata:

```csharp
trace.ShouldRecordRequest<CreateOrderCommand>();
```

Short-circuit behavior:

```csharp
trace.ShouldNotContain("HandlerExecution");
```

Transaction behavior:

```csharp
var transactionTrace = trace.Transaction;

transactionTrace.ShouldBegin();
transactionTrace.ShouldCommit();
transactionTrace.ShouldRollback();
```

## Failure Simulation

Simulate a failure inside a specific boundary:

```csharp
spider.FailInside<SomeBoundary>(new InvalidOperationException());
```

When a boundary failure is simulated, Spider testing records the boundary failure and short-circuits before handler execution.

## Integration

External test hosts can wrap the registration with a small adapter:

```csharp
testHost.UseSpiderTesting(typeof(TransactionBoundary).Assembly);
```

The wrapper only needs to forward assemblies to:

```csharp
services.AddSpiderTesting(assemblies);
```
