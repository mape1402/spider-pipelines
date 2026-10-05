# ComposeFlow Design And Implementation

## Status

This document describes the `ComposeFlow` architecture inside Spider and the architecture metadata generated at compile time.

The current implementation includes a source generator in `Spider.Pipelines.Analyzers`. The generator extracts flows and pipelines during compilation and produces `SpiderGeneratedArchitecture.BuildManifest()` without running the application.

This document replaces the earlier discussion around isolated naming ideas. The goal is no longer just to have a pleasant builder API. The goal is for Spider to describe local business processes and generate static metadata without mixing that responsibility with the global pipelines that already exist.

## Problem To Solve

Spider currently models pipelines as a cross-cutting execution wrapper. That is useful for global concerns:

- boundaries;
- preprocessors;
- postprocessors;
- middleware;
- parallel execution;
- target execution;
- tracing, logging, transactions, and retries.

However, it does not precisely describe the business pseudocode inside a method.

Example:

```txt
CreateCustomer
  Validate
  Map
  Save
  ReturnResponse
```

That local flow should not live as a global pipeline. It is the method's business logic. The value of `ComposeFlow` is to make that logic:

- readable;
- typed;
- simple to adopt;
- documentable with analyzers and source generators;
- observable at runtime in a later phase.

## Metadata And Telemetry

The architecture separates two planes:

```txt
Definition plane:
  manifests
  components
  relations
  evidence

Execution plane:
  runtime events
  traces
  failures
  branch decisions

Producer:
  Spider
  pipelines
  ComposeFlow
  boundaries
  source generator
  telemetry adapter
```

`ComposeFlow` must produce two kinds of information.

### Static Metadata

Build-time, through an analyzer/source generator.

It must publish a semantic manifest with:

- `spider.flow` components;
- `spider.execute-flow` and `spider.flow-step` operations;
- `contains`, `next`, `branches-to`, `uses`, `returns`, and `calls-flow` relations;
- CLR/source evidence;
- warnings and diagnostics;
- specialized Spider metadata.

### Telemetry For RavenTracer

Runtime, when the flow executes, in a later phase.

It must emit semantic events:

- flow started, completed, faulted, and cancelled;
- step started, completed, and faulted;
- condition evaluated;
- branch selected;
- early return;
- nested flow started and completed.

Documentation must not depend on runtime execution. Runtime is for telemetry.

## Separation From Pipelines

Pipelines:

```txt
Endpoint
  Spider pipeline
    boundaries
    preprocessors
    target
    postprocessors
```

ComposeFlow:

```txt
ServiceMethod
  ComposeFlow
    Validate
    Map
    Save
    ReturnResponse
```

An endpoint can be wrapped by a pipeline and then execute handlers/services that use `ComposeFlow`.

Example:

```txt
POST /loan/evaluation
  spider.pipeline:http.loan-evaluation
    pelican.handler:LoanEvaluationCommandHandler
      spider.flow:loan.evaluate
        CheckCreditBureau
          spider.flow:bureau.check-status
            BuildRequest
            CallRemoteService
            EvaluateResponse
        EvaluateRisk
          spider.flow:risk.evaluate
            BuildRequest
            CalculateRisk
            ReturnScore
        PrepareReport
        ReturnReport
```

The manifest must be able to represent the static map. Runtime telemetry must be able to represent the real execution in a later phase.

## API Principles

1. The flow must read like pseudocode.
2. The first step does not need a special name.
3. There must be no public Spider `Context`.
4. It must not force `Result<T>`.
5. It must not magically resolve any parameter from any previous output.
6. If a step needs previous values, it must say so explicitly.
7. If the flow declares a response, every early exit must respect `TResponse` or throw an exception.
8. Flows without a response must be first-class citizens.
9. Branches must converge to a common signature or terminate the flow.
10. Static metadata and runtime telemetry are separate concerns.

## Recommended Vocabulary

```csharp
ComposeFlow(...)
UsingProfile(...)
Then(...)
ThenWith<T1>(...)
ThenWith<T1, T2>(...)
ThenWith<T1, T2, T3>(...)
ContinueIf(...)
Branch(...)
When(...)
Otherwise(...)
RunAsync(...)
```

Do not use these names as the primary API:

```txt
StartWith
Do
SwitchTo
Connect
Select
Project
Require
Return
End
Reject
```

Reasoning:

- `Then` reads as the next step.
- `ThenWith` reads as the next step using additional values.
- `ContinueIf` communicates a continuity guard.
- `Branch` communicates a flow split.
- `Return` and `End` are unnecessary when `Then` and `RunAsync` already express the final output rules.
- `Reject` must not be a generic outcome because it breaks contracts for flows with `TResponse`.

## Flows With And Without A Response

There must be two families.

```csharp
ComposeFlow<TRequest>(string name)
ComposeFlow<TRequest, TResponse>(string name)
```

Flow with response:

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

Flow without response:

```csharp
await spider
    .ComposeFlow<CreateCustomerRequest>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .RunAsync(request, cancellationToken);
```

Rules:

- `ComposeFlow<TRequest>` returns `Task` or `ValueTask`.
- `ComposeFlow<TRequest, TResponse>` returns `Task<TResponse>` or `ValueTask<TResponse>`.
- In flows without a response, the final active signature can be any type.
- In flows with a response, the final active signature must be `TResponse`.
- If `TResponse` is missing, analyzer/build validation and runtime validation must fail.

## Active Signature And History

The runtime maintains two internal concepts.

```txt
Active signature:
  The value that Then uses by default.

History:
  Initial values and previous outputs.
  Used only when the user declares that need with ThenWith.
```

Example:

```csharp
spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

Methods:

```csharp
Task Validate(CreateCustomerRequest request, CancellationToken cancellationToken);

Task<Customer> Map(CreateCustomerRequest request, CancellationToken cancellationToken);

Task Save(
    CreateCustomerRequest request,
    Customer customer,
    CancellationToken cancellationToken);

Task<CustomerResponse> ReturnResponse(
    Customer customer,
    CancellationToken cancellationToken);
```

Execution:

```txt
Initial:
  active = CreateCustomerRequest
  history = CreateCustomerRequest

Then(Validate):
  uses active CreateCustomerRequest
  returns void
  active remains CreateCustomerRequest

Then(Map):
  uses active CreateCustomerRequest
  returns Customer
  active = Customer
  history += Customer

ThenWith<CreateCustomerRequest, Customer>(Save):
  uses explicit history values
  returns void
  active remains Customer

Then(ReturnResponse):
  uses active Customer
  returns CustomerResponse
  active = CustomerResponse
```

## Then

`Then` executes the next step using the active signature.

It is also used as the first step.

Base signatures:

```csharp
.Then(Func<TCurrent, TNext> step)
.Then(Func<TCurrent, CancellationToken, Task<TNext>> step)
.Then(Func<TCurrent, CancellationToken, Task> step)
.Then(Func<TCurrent, ValueTask<TNext>> step)
.Then(Func<TCurrent, CancellationToken, ValueTask<TNext>> step)
.Then(Func<TCurrent, CancellationToken, ValueTask> step)
```

Rules:

- If the step returns a value, that value becomes the active signature.
- If the step does not return a value, the active signature is preserved.
- If the step throws, the flow becomes `faulted`.
- If the token is cancelled, the flow becomes `cancelled`.

Exception-based validation:

```csharp
.Then(Validate)
```

If `Validate` throws, the flow fails. That does not require `ContinueIf`.

## ThenWith

`ThenWith` executes the next step using specific values from the history.

```csharp
.ThenWith<CreateCustomerRequest, Customer>(Save)
```

It reads as:

```txt
Then save with CreateCustomerRequest and Customer.
```

Signatures:

```csharp
.ThenWith<T1>(Func<T1, TNext> step)
.ThenWith<T1>(Func<T1, CancellationToken, Task<TNext>> step)
.ThenWith<T1>(Func<T1, CancellationToken, Task> step)

.ThenWith<T1, T2>(Func<T1, T2, TNext> step)
.ThenWith<T1, T2>(Func<T1, T2, CancellationToken, Task<TNext>> step)
.ThenWith<T1, T2>(Func<T1, T2, CancellationToken, Task> step)

.ThenWith<T1, T2, T3>(Func<T1, T2, T3, TNext> step)
.ThenWith<T1, T2, T3>(Func<T1, T2, T3, CancellationToken, Task<TNext>> step)
.ThenWith<T1, T2, T3>(Func<T1, T2, T3, CancellationToken, Task> step)
```

Rules:

- Requested types must exist in the history.
- Exact match wins over assignable match.
- If there are multiple candidates, the request is ambiguous.
- If the step returns a value, it updates the active signature.
- If the step does not return a value, the active signature is preserved.

## ContinueIf

`ContinueIf` is a continuity guard. It only applies to boolean predicates.

There must not be an overload with `Action` or `Task`, because that is already `Then(Validate)`.

Rule:

```txt
true  -> continue
false -> apply otherwise
throw -> fault
```

For flows with a response:

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .Then(Normalize)
    .ContinueIf(
        HasValidEmail,
        otherwise: Flow.Return(BuildInvalidEmailResponse))
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

It can also fail with an exception:

```csharp
.ContinueIf(
    HasValidEmail,
    otherwise: Flow.Throw(() => new ValidationException("Email is required")))
```

For flows without a response:

```csharp
await spider
    .ComposeFlow<SendWelcomeEmailCommand>("Send welcome email")
    .ContinueIf(
        ShouldSendWelcomeEmail,
        otherwise: Flow.Stop())
    .Then(SendEmail)
    .RunAsync(command, cancellationToken);
```

Allowed outcomes:

```txt
Flow.Return(...)  -> only flows with TResponse
Flow.Throw(...)   -> flows with or without TResponse
Flow.Stop()       -> only flows without TResponse
```

Do not include `Flow.Reject` in v1.

Reason:

```txt
Reject without TResponse breaks the ComposeFlow<TRequest, TResponse> contract.
If the consumer wants a semantic rejected result, it must be modeled in TResponse.
```

Example:

```csharp
ComposeFlow<CreateCustomerRequest, CreateCustomerResult>
```

where `CreateCustomerResult` can represent success, validation failure, or any other domain outcome.

## Branch

`Branch` splits the flow into routes.

Example that converges:

```csharp
return spider
    .ComposeFlow<LoanApplication, RiskDecision>("Evaluate loan")
    .Then(Validate)
    .Then(BuildProfile)
    .Branch(branch => branch
        .When(IsLowRisk, low => low
            .Then(AutoApprove))
        .When(IsHighRisk, high => high
            .Then(RequireManualReview))
        .Otherwise(normal => normal
            .Then(CalculateStandardDecision)))
    .Then(ReturnDecision)
    .RunAsync(application, cancellationToken);
```

Rules:

- Each branch receives the current active signature.
- Each route can use `Then`, `ThenWith`, `ContinueIf`, and `Branch`.
- If there are steps after `Branch`, all routes must converge to the same active type.
- If `Branch` is the final element of a flow without a response, it may end with steps that return no value.
- If `Branch` is the final element of a flow with a response, each route must produce `TResponse` or throw an exception.
- `Otherwise` must be mandatory in v1 to avoid uncovered routes.

Terminal branch without response:

```csharp
await spider
    .ComposeFlow<NotificationCommand>("Send notification")
    .Then(Validate)
    .Branch(branch => branch
        .When(IsEmail, email => email.Then(SendEmail))
        .When(IsSms, sms => sms.Then(SendSms))
        .Otherwise(other => other.Then(LogUnsupportedChannel)))
    .RunAsync(command, cancellationToken);
```

Terminal branch with response:

```csharp
return spider
    .ComposeFlow<PaymentRequest, PaymentResponse>("Process payment")
    .Then(Validate)
    .Branch(branch => branch
        .When(IsFreeOrder, free => free.Then(ReturnFreeOrderResponse))
        .Otherwise(paid => paid
            .Then(ChargeCard)
            .Then(ReturnPaidOrderResponse)))
    .RunAsync(request, cancellationToken);
```

`Return` and `End` are not needed. The contract is determined by the flow's expected final type and the branch position.

## Duplicate Types

If two steps produce the same type, `ThenWith<T>` can be ambiguous.

Example:

```csharp
.Then(LoadPrimaryCustomer)
.Then(LoadSecondaryCustomer)
.ThenWith<Customer>(ValidateCustomer)
```

Options:

1. Avoid duplicate values of the same type.
2. Create distinct domain types.
3. Create an explicit state record.
4. Use a name as an escape hatch.

Escape hatch:

```csharp
.Then(LoadPrimaryCustomer).NameOutput("primaryCustomer")
.Then(LoadSecondaryCustomer).NameOutput("secondaryCustomer")
.ThenWith<Customer>("primaryCustomer", ValidateCustomer)
```

This must not be the happy path.

## Explicit Operation State

When the flow needs too many live values, create a domain state record.

```csharp
public sealed record CreateCustomerState(
    CreateCustomerRequest Request,
    Customer Customer,
    CustomerPolicy Policy);
```

Usage:

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(BuildState)
    .Then(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

This is not a Spider `Context`. It is domain state controlled by the application.

## Supported Arity

Recommended v1 limit:

```txt
ThenWith up to 3 values.
ComposeFlow up to 3 initial inputs.
```

If more than 3 values are required, use explicit state.

Reason:

```txt
Readability drops quickly with high arity.
The goal is simple pseudocode, not an alternate programming language.
```

## Profiles

Profiles are reusable runtime configuration.

Example:

```csharp
services.AddSpider(spider =>
{
    spider.AddFlowProfile("Business", profile =>
    {
        profile.EnableTelemetry();
        profile.EnableMetrics();
    });

    spider.AddFlowProfile("ExternalService", profile =>
    {
        profile.EnableTelemetry();
        profile.EnableMetrics();
        profile.UseBoundary<ExternalServiceBoundary>();
    });
});
```

Usage:

```csharp
spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

Profile responsibilities:

- telemetry on/off;
- metrics on/off;
- logging verbosity;
- redaction policy;
- boundaries;
- event enrichment;
- environment-based behavior;
- feature flags.

Profiles must not change business semantics by default.

## Runtime Architecture In Spider

Proposed components inside Spider:

```txt
Spider.Pipelines.Core
  ISpider
  Spider

Spider.Pipelines.Flows
  ISpiderFlow<TRequest>
  ISpiderFlow<TRequest, TResponse>
  ISpiderFlowBuilder<...>
  SpiderFlowBuilder
  SpiderFlowDefinition
  SpiderFlowStep
  SpiderFlowExecutionPlan
  SpiderFlowExecutor
  SpiderFlowHistory
  SpiderFlowProfile
  SpiderFlowProfileOptions

Spider.Pipelines.Flows.Internals
  DelegateStepInvoker
  StepReturnClassifier
  FlowHistoryResolver
  BranchExecutor
  ContinueIfExecutor
  FlowValidation
```

### ISpider

Add:

```csharp
ISpiderFlow<TRequest> ComposeFlow<TRequest>(
    string name);

ISpiderFlow<TRequest, TResponse> ComposeFlow<TRequest, TResponse>(
    string name);

ISpiderFlow<TRequest1, TRequest2> ComposeFlow<TRequest1, TRequest2>(
    string name);

ISpiderFlow<TRequest1, TRequest2, TResponse> ComposeFlow<TRequest1, TRequest2, TResponse>(
    string name);
```

Multiple-input overloads must be handled carefully so the API does not explode with generic combinations. V1 can start with one and two inputs.

### Flow Definition

Represents the runtime structure:

```txt
Name
ProfileName
InputTypes
ResponseType?
Steps[]
Branches[]
Evidence?
StaticDescriptorId?
```

### Flow Step

```txt
StepId
DisplayName
Kind
Delegate
UsesTypes[]
OutputType?
PreservesActiveWhenNoOutput
Evidence
Metadata
```

### Flow History

Internal runtime bag:

```txt
entries:
  type
  optional name
  value
  source step id
```

It is not public.

### Step Invoker

Responsibilities:

- resolve parameters from the active value or explicit history;
- inject `CancellationToken`;
- invoke sync/async delegates;
- classify return values;
- update active value/history;
- preserve the original exception.

## Analyzer And Source Generator

There must be an analyzer/source generator project inside the Spider solution.

It is not required for runtime execution, but it is required to generate static metadata and early diagnostics.

Candidate package:

```txt
Spider.Pipelines.Analyzers
```

Note:

```txt
ComposeFlow lives in Spider.
The analyzer can be packaged as an analyzer in the same NuGet package or as a companion package if that decision changes later.
The runtime API must not depend on the analyzer.
```

### Analyzer Responsibilities

Validate:

- flow without a name;
- unknown profile when resolvable;
- `Then` incompatible with the active signature;
- `ThenWith` requesting a type that is not available in history;
- ambiguous `ThenWith`;
- response flow final output does not produce `TResponse`;
- `ContinueIf` without a valid otherwise outcome;
- `Flow.Stop()` used in a flow with `TResponse`;
- `Flow.Return(...)` incompatible with `TResponse`;
- branch without `Otherwise`;
- branches that do not converge;
- anonymous lambda with limited metadata;
- arity above the recommended limit.

### Source Generator Responsibilities

Generate static architecture descriptors.

Conceptual output:

```csharp
internal static partial class SpiderGeneratedArchitecture
{
    public static ArchitectureManifest BuildManifest()
    {
        ...
    }
}
```

The generator must not execute user code.

It must use:

- syntax tree;
- semantic model;
- method group symbols;
- generic arguments;
- attributes;
- source location.

## Static Metadata

Spider must publish manifests using its own typed models.

### Component Kinds

```txt
spider.pipeline
spider.flow
spider.flow-step
spider.flow-branch
spider.flow-condition
spider.flow-profile
```

### Operation Kinds

```txt
spider.run-pipeline
spider.execute-flow
spider.execute-step
spider.evaluate-condition
spider.evaluate-branch
```

### Relation Kinds

```txt
contains
next
uses
uses-profile
branches-to
otherwise
returns
throws
calls-flow
wrapped-by
observes
```

### Evidence

Each step must include evidence when possible:

```txt
SourceKind: source-generator
ClrTypeName
MemberName
FilePath
LineNumber
DiscoveryMethod: source-generator
```

### Descriptor Example

```json
{
  "producer": "Spider.Pipelines",
  "components": [
    {
      "id": "spider.flow:create-customer",
      "kind": "spider.flow",
      "displayName": "Create customer",
      "metadata": {
        "input": "CreateCustomerRequest",
        "output": "CustomerResponse",
        "profile": "Business"
      }
    },
    {
      "id": "spider.flow-step:create-customer.validate",
      "kind": "spider.flow-step",
      "displayName": "Validate",
      "metadata": {
        "uses": ["CreateCustomerRequest"],
        "output": "CreateCustomerRequest"
      }
    }
  ],
  "relations": [
    {
      "sourceId": "spider.flow:create-customer",
      "targetId": "spider.flow-step:create-customer.validate",
      "kind": "contains",
      "confidence": "explicit"
    },
    {
      "sourceId": "spider.flow-step:create-customer.validate",
      "targetId": "spider.flow-step:create-customer.map",
      "kind": "next",
      "confidence": "explicit"
    }
  ]
}
```

## Telemetry For RavenTracer

Spider must emit runtime events if the profile enables them.

Events:

```txt
spider.flow.started
spider.flow.completed
spider.flow.faulted
spider.flow.cancelled
spider.step.started
spider.step.completed
spider.step.faulted
spider.condition.evaluated
spider.branch.evaluated
spider.branch.selected
```

Fields:

```txt
ComponentId = spider.flow:<flow-id> or spider.flow-step:<step-id>
OperationId = spider.execute-flow / spider.execute-step
DefinitionVersion
DeploymentId
TraceId
SpanId
ParentSpanId
CorrelationId
Status
Duration
Exception
Properties
```

Payloads must be disabled by default.

Payloads, inputs, and outputs must be exposed only through a redaction policy/profile.

## Recommended Fluent API

### Simple Case With Response

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .Then(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

### Using Previous Values

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

### Without Response

```csharp
await spider
    .ComposeFlow<CreateCustomerRequest>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .RunAsync(request, cancellationToken);
```

### ContinueIf With Early Response

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .Then(Normalize)
    .ContinueIf(
        HasValidEmail,
        otherwise: Flow.Return(BuildInvalidEmailResponse))
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

### ContinueIf With Exception

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .Then(Normalize)
    .ContinueIf(
        HasValidEmail,
        otherwise: Flow.Throw(() => new ValidationException("Email is required")))
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

### ContinueIf In A Flow Without Response

```csharp
await spider
    .ComposeFlow<SendWelcomeEmailCommand>("Send welcome email")
    .ContinueIf(
        ShouldSendEmail,
        otherwise: Flow.Stop())
    .Then(SendEmail)
    .RunAsync(command, cancellationToken);
```

### Converging Branch

```csharp
return spider
    .ComposeFlow<LoanApplication, RiskDecision>("Evaluate loan")
    .Then(Validate)
    .Then(BuildProfile)
    .Branch(branch => branch
        .When(IsLowRisk, low => low.Then(AutoApprove))
        .When(IsHighRisk, high => high.Then(RequireManualReview))
        .Otherwise(normal => normal.Then(CalculateStandardDecision)))
    .Then(ReturnDecision)
    .RunAsync(application, cancellationToken);
```

### Terminal Branch With Response

```csharp
return spider
    .ComposeFlow<PaymentRequest, PaymentResponse>("Process payment")
    .Then(Validate)
    .Branch(branch => branch
        .When(IsFreeOrder, free => free.Then(ReturnFreeOrderResponse))
        .Otherwise(paid => paid
            .Then(ChargeCard)
            .Then(ReturnPaidOrderResponse)))
    .RunAsync(request, cancellationToken);
```

### Terminal Branch Without Response

```csharp
await spider
    .ComposeFlow<NotificationCommand>("Send notification")
    .Then(Validate)
    .Branch(branch => branch
        .When(IsEmail, email => email.Then(SendEmail))
        .When(IsSms, sms => sms.Then(SendSms))
        .Otherwise(other => other.Then(LogUnsupportedChannel)))
    .RunAsync(command, cancellationToken);
```

### Explicit State

```csharp
return spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(BuildState)
    .Then(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

## Metadata Generation Plan

Full documentation metadata must be generated at build time. Runtime remains reserved for telemetry.

### Implemented State

Spider already publishes an architecture manifest at compile time:

- `spider.pipeline` for pipelines configured with `Attach`;
- `spider.pipeline-stage` for preprocess, middleware, target, parallel, success postprocess, and failure postprocess stages;
- `spider.flow` for flows composed with `ComposeFlow`;
- `spider.flow-step`, `spider.flow-condition`, and `spider.flow-branch` for flow steps;
- `spider.flow-profile` for profiles used with `UsingProfile`;
- `contains`, `next`, and `uses-profile` relations;
- file, line, type, and member evidence when the compiler can resolve it;
- `SpiderGeneratedArchitecture.BuildManifest()` as the generated read API.

There is no runtime discovery for documentation. If a flow exists in source code, the generator must be able to see it even when the flow is not executed.

### First Cut Goal

Generate a static manifest with:

- flows declared with `ComposeFlow`;
- steps added with `Then`;
- steps added with `ThenWith`;
- conditions added with `ContinueIf`;
- branches added with `Branch`, `When`, and `Otherwise`;
- profiles used with `UsingProfile`;
- file, line, CLR type, and method evidence;
- diagnostics when the flow cannot be documented well.

Do not try to generate telemetry from the source generator. That belongs to RavenTracer/runtime.

### Suggested Project

Create an analyzer/source generator project:

```txt
src/Spider.Pipelines.Analyzers
```

This project should be packaged as an analyzer inside `Spider.Pipelines` or as a companion package. Recommended v1 decision:

```txt
Package it inside Spider.Pipelines as an included analyzer.
```

Reason:

- users get diagnostics without installing another package;
- the manifest is generated automatically;
- the feature feels like part of Spider;
- it adds no runtime dependencies to the main package.

The Spider runtime must not depend on the analyzer.

### Metadata Contracts

Spider publishes stable contracts so the metadata is not coupled to any external consumer.

Suggested namespace:

```txt
Spider.Pipelines.Architecture
```

Initial internal or public types:

```csharp
SpiderArchitectureManifest
SpiderComponentDescriptor
SpiderOperationDescriptor
SpiderRelationDescriptor
SpiderEvidenceDescriptor
SpiderDiagnosticDescriptor
SpiderMetadataBag
```

Do not couple runtime to these contracts beyond the public models required to read the generated manifest.

### Stable IDs

The source generator must create logical IDs. CLR identity is evidence, not the primary identity.

Recommended format:

```txt
spider.flow:{normalized-flow-name}
spider.flow-step:{normalized-flow-name}.{step-name-or-index}
spider.flow-branch:{normalized-flow-name}.{branch-index}
spider.flow-condition:{normalized-flow-name}.{condition-name-or-index}
spider.flow-profile:{profile-name}
```

Example:

```txt
spider.flow:create-customer
spider.flow-step:create-customer.validate
spider.flow-step:create-customer.map
spider.flow-step:create-customer.save
spider.flow-step:create-customer.return-response
spider.flow-profile:business
```

If the same name appears more than once in the same scope, add a stable position suffix:

```txt
spider.flow-step:create-customer.validate-2
```

Do not use line number as part of the ID because that breaks stability when formatting changes. Line number belongs only in evidence.

### Evidence

Each descriptor must include evidence when possible:

```txt
SourceKind: source-generator
RepositoryPath
ProjectName
AssemblyName
ClrTypeName
MemberName
FilePath
LineNumber
DiscoveryMethod: source-generator
```

For method groups:

```csharp
.Then(Validate)
```

Evidence:

```txt
MemberName: Validate
ClrTypeName: CreateCustomerService
FilePath: ...
LineNumber: ...
```

For lambdas:

```csharp
.Then(request => ...)
```

Evidence:

```txt
MemberName: <lambda>
FilePath: ...
LineNumber: ...
Diagnostic: lambda has limited metadata
```

### Component Kinds

```txt
spider.flow
spider.flow-step
spider.flow-branch
spider.flow-condition
spider.flow-profile
```

### Operation Kinds

```txt
spider.execute-flow
spider.execute-step
spider.evaluate-condition
spider.evaluate-branch
```

### Relation Kinds

```txt
contains
next
uses
uses-profile
branches-to
otherwise
returns
throws
calls-flow
```

### Descriptor Shape

Conceptual example:

```json
{
  "producer": "Spider.Pipelines",
  "manifestVersion": "1.0",
  "components": [
    {
      "id": "spider.flow:create-customer",
      "kind": "spider.flow",
      "displayName": "Create customer",
      "metadata": {
        "input": "CreateCustomerRequest",
        "output": "CustomerResponse",
        "profile": "Business"
      }
    },
    {
      "id": "spider.flow-step:create-customer.save",
      "kind": "spider.flow-step",
      "displayName": "Save",
      "metadata": {
        "uses": [
          "CreateCustomerRequest",
          "Customer"
        ],
        "output": "Customer",
        "preservesActiveValue": true
      }
    }
  ],
  "relations": [
    {
      "sourceId": "spider.flow:create-customer",
      "targetId": "spider.flow-step:create-customer.save",
      "kind": "contains",
      "confidence": "explicit"
    }
  ]
}
```

### Source Generator Output

The generator must emit a file like:

```csharp
// <auto-generated />
namespace Spider.Pipelines.Generated
{
    internal static partial class SpiderGeneratedArchitecture
    {
        public static SpiderArchitectureManifest BuildManifest()
        {
            ...
        }
    }
}
```

It may also emit descriptors per assembly:

```csharp
internal static partial class SpiderGeneratedArchitecture_AssemblyName
```

The consuming project can read the generated manifest directly:

```csharp
var manifest = SpiderGeneratedArchitecture.BuildManifest();
```

### Analyzer Diagnostics

Recommended diagnostics:

```txt
SPF001 Flow name is required.
SPF002 Flow response contract is not satisfied.
SPF003 Then cannot bind to the active type.
SPF004 ThenWith references a type that is not available in history.
SPF005 ThenWith is ambiguous because multiple values match.
SPF006 Branch requires Otherwise.
SPF007 Branch routes do not converge.
SPF008 ContinueIf requires an outcome compatible with the flow contract.
SPF009 Flow.Stop cannot be used in a flow with TResponse.
SPF010 Lambda step has limited documentation metadata.
SPF011 Flow profile is not known.
SPF012 Unsupported ComposeFlow shape.
```

Initial severity:

- contract errors: error;
- incomplete metadata: warning;
- unknown profile: warning at first, error if strict policy is enabled.

### How To Analyze The Fluent API

The analyzer must find invocations to:

```csharp
ComposeFlow(...)
```

Then it must walk the fluent chain:

```txt
ComposeFlow
  -> UsingProfile?
  -> Then*
  -> ThenWith*
  -> ContinueIf*
  -> Branch*
  -> RunAsync
```

It must resolve semantically:

- generic arguments;
- receiver type after each method;
- method group symbol;
- lambda syntax;
- return type;
- `CancellationToken` parameter;
- branch route chains.

It must not execute code.

### Phase Strategy

#### Metadata Phase 1: Linear

Support:

- `ComposeFlow<TRequest>`;
- `ComposeFlow<TRequest, TResponse>`;
- `UsingProfile`;
- `Then`;
- `ThenWith`;
- `RunAsync`.

Generate:

- flow component;
- step components;
- contains relations;
- next relations;
- uses-profile relation;
- input/output metadata.

#### Metadata Phase 2: ContinueIf

Support:

- `ContinueIf(..., Flow.Return(...))`;
- `ContinueIf(..., Flow.Throw(...))`;
- `ContinueIf(..., Flow.Stop())`.

Generate:

- condition component;
- `next` relation to the condition;
- `returns` or `throws` relation;
- early-exit metadata.

#### Metadata Phase 3: Branch

Support:

- `Branch`;
- `When`;
- `Otherwise`;
- nested linear route chains.

Generate:

- branch component;
- condition components;
- `branches-to`;
- `otherwise`;
- convergence metadata.

#### Metadata Phase 4: Nested Flows

Detect when a step calls another method that contains `ComposeFlow`.

First version:

- only when the source generator can see the called method in the same compilation.

Generate:

```txt
calls-flow
```

If it cannot resolve the target:

```txt
unresolved reference warning
```

#### Metadata Phase 5: Elysium Protocol Adapter

Map:

```txt
SpiderArchitectureManifest -> ArchitectureManifest
SpiderComponentDescriptor -> ComponentDescriptor
SpiderRelationDescriptor -> RelationDescriptor
SpiderEvidenceDescriptor -> EvidenceDescriptor
```

Expose models that can be adapted by external consumers.

### Acceptance Criteria

The first cut is ready when:

- a consuming project that uses `ComposeFlow` generates a manifest during build;
- the manifest contains flow, steps, and relations;
- method groups include CLR/source evidence;
- lambdas generate a limited metadata warning;
- invalid `ThenWith` produces a diagnostic;
- a flow with an unsatisfied `TResponse` produces a diagnostic;
- tests can read the manifest without executing the flow;
- runtime still works without the analyzer.

### Required Tests

Create analyzer/source generator tests with `Microsoft.CodeAnalysis.CSharp.Testing`.

Cases:

- linear flow with response generates the correct manifest;
- flow without response generates the correct manifest;
- `ThenWith` generates uses metadata;
- `UsingProfile` generates a `uses-profile` relation;
- lambda generates warning;
- branch generates branch descriptors;
- invalid flow generates diagnostic;
- generated manifest compiles.

## Implementation Order

### Phase 1: Basic API And Runtime

- Add `ComposeFlow<TRequest>` and `ComposeFlow<TRequest, TResponse>` to `ISpider`.
- Implement `Then`.
- Implement `ThenWith` up to arity 2.
- Implement flows with and without response.
- Implement `RunAsync`.
- Validate final output in flows with response.
- Add tests for sync/async/value/no-value steps.

### Phase 2: Continuity Guards

- Implement `ContinueIf`.
- Implement `Flow.Return`.
- Implement `Flow.Throw`.
- Implement `Flow.Stop` only for flows without response.
- Validate contract errors.

### Phase 3: Branches

- Implement `Branch`, `When`, and `Otherwise`.
- Validate convergence.
- Validate terminal branches with and without response.
- Add basic nested branch tests.

### Phase 4: Profiles

- Add `AddFlowProfile`.
- Add `UsingProfile`.
- Add telemetry, metrics, logging, redaction, and boundary options.
- Preserve business semantics.

### Phase 5: Static Metadata

- Create flow, step, branch, and condition descriptors.
- Create stable IDs.
- Add compilation evidence.
- Prepare a shape compatible with external consumers.

### Phase 6: Analyzer/Source Generator

- Implement diagnostics.
- Generate the static manifest.
- Generate IDs, relations, and evidence.
- Expose the generated manifest through `SpiderGeneratedArchitecture.BuildManifest()`.

### Phase 7: Telemetry Adapter

- Emit runtime events.
- Correlate nested flows.
- Correlate flows inside pipelines.
- Expose an adapter for RavenTracer.

### Phase 8: External Integration

- Create `SpiderTelemetryAdapter`.
- Test the first vertical:

```txt
Pigeon Consumer
  -> Spider Pipeline
  -> Handler
  -> Spider ComposeFlow
  -> Validator/Mapper/Outbox
```

## V1 Decisions

- `ComposeFlow` lives inside Spider.
- `Then` is the normal step.
- `ThenWith` uses explicitly declared previous values.
- `ContinueIf` accepts only boolean predicates.
- Exception-based validation is modeled with `Then`.
- There is no core `Reject` outcome.
- A flow with response can end only with `TResponse` or an exception.
- A flow without response can end with `Flow.Stop`.
- Branches must include `Otherwise` in v1.
- Branches must converge or be terminal.
- Runtime does not generate documentation.
- The source generator/analyzer generates static metadata.
- Runtime events feed RavenTracer.

## Risks

### C# Inference

Some combinations of method groups and generics may require specific overloads.

Mitigation:

- start with arity 1 and 2;
- add helpers only when real cases require them;
- prefer clear errors over magic.

### History Ambiguity

The same type can be produced more than once.

Mitigation:

- fail by default;
- recommend state records;
- use `NameOutput` as an escape hatch.

### Source Generator Complexity

Analyzing fluent chains with branches can grow quickly.

Mitigation:

- phase the generator after runtime;
- support the linear shape first;
- add branches later.

### Too Much Power In Branch

It can become an alternate language.

Mitigation:

- no complex joins in v1;
- no arbitrary graph;
- branches either converge or terminate.

## Expected Result

With `ComposeFlow`, Spider can publish a manifest like:

```txt
spider.flow:create-customer
  contains Validate
  next Map
  next Save
  next ReturnResponse
  uses-profile Business
```

And RavenTracer can show:

```txt
Create customer started
  Validate completed
  Map completed
  Save faulted
Create customer faulted
```

With correlation:

```txt
Definition: spider.flow:create-customer
Runtime: trace 7f3...
Failure: spider.flow-step:create-customer.save
Evidence: CreateCustomerService.Save, file/line
```

That is the real point: not just chaining lambdas, but turning local business processes into queryable architecture and observable execution.
