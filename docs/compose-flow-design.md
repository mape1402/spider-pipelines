# Diseno E Implementacion De ComposeFlow

## Estado

Arquitectura de `ComposeFlow` dentro de Spider y plan evolutivo para integrarse despues con KnOwl y RavenTracer.

El corte actual ya implementa metadata por composicion runtime para flows y pipelines a traves de `ISpiderArchitectureProvider`. El analyzer/source generator sigue siendo la ruta recomendada para metadata build-time con evidence de archivo/linea y diagnosticos estaticos.

Este documento reemplaza la discusion previa de nombres sueltos. La meta ya no es solo tener un builder bonito. La meta es que Spider pueda describir procesos de negocio locales, generar metadata estatica para KnOwl y emitir telemetria runtime para RavenTracer sin mezclar eso con los pipelines globales que ya existen.

## Problema A Resolver

Spider actualmente modela pipelines como una envoltura transversal de ejecucion. Eso sirve para concerns globales:

- boundaries;
- pre-processors;
- post-processors;
- middleware;
- parallel execution;
- target execution;
- tracing/logging/transactions/retries.

Pero no describe con precision el pseudocodigo de negocio dentro de un metodo.

Ejemplo:

```txt
CreateCustomer
  Validate
  Map
  Save
  ReturnResponse
```

Ese flujo local no debe vivir como pipeline global. Es logica de negocio del metodo. El valor de `ComposeFlow` es volver esa logica:

- legible;
- tipada;
- simple de adoptar;
- documentable con analyzers/source generators;
- observable en runtime;
- correlacionable con KnOwl/RavenTracer.

## Relacion Con KnOwl Y RavenTracer

La arquitectura objetivo separa tres planos:

```txt
Definition plane:
  KnOwl
  manifests
  components
  operations
  relations
  evidence

Execution plane:
  RavenTracer
  runtime events
  traces
  spans
  attempts
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

`ComposeFlow` debe producir dos tipos de informacion.

### Metadata Para KnOwl

Build-time, mediante analyzer/source generator.

Debe publicar un manifest semantico con:

- componente `spider.flow`;
- operaciones `spider.execute-flow` y `spider.flow-step`;
- relaciones `contains`, `next`, `branches-to`, `uses`, `returns`, `calls-flow`;
- evidence de CLR/source;
- warnings/diagnostics;
- metadata especializada de Spider.

### Telemetria Para RavenTracer

Runtime, cuando el flow se ejecuta.

Debe emitir eventos semanticos:

- flow started/completed/faulted/cancelled;
- step started/completed/faulted;
- condition evaluated;
- branch selected;
- early return;
- nested flow started/completed.

La documentacion no debe depender de runtime. Runtime es para telemetria.

## Separacion Con Pipelines

Pipelines:

```txt
Endpoint
  Spider pipeline
    boundaries
    pre-processors
    target
    post-processors
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

Un endpoint puede estar envuelto por un pipeline y dentro ejecutar handlers/services que usan `ComposeFlow`.

Ejemplo:

```txt
POST /loan/evaluation
  spider.pipeline:http.loan-evaluation
    pelican.handler:LeanEvaluationCommandHandler
      spider.flow:loan.evaluate
        CheckBuroCredito
          spider.flow:buro.check-status
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

KnOwl debe poder ver el mapa estatico. RavenTracer debe poder ver la ejecucion real.

## Principios De API

1. El flow debe leerse como pseudocodigo.
2. El primer step no necesita nombre especial.
3. No debe existir `Context` publico de Spider.
4. No debe obligar `Result<T>`.
5. No debe resolver magicamente cualquier parametro desde cualquier output previo.
6. Si un step necesita valores previos, debe decirlo explicitamente.
7. Si el flow declara respuesta, toda salida temprana debe respetar `TResponse` o lanzar excepcion.
8. Flows sin respuesta deben ser ciudadanos de primera clase.
9. Branches deben converger a una firma comun o terminar el flow.
10. Metadata estatica y telemetria runtime son cosas separadas.

## Vocabulario Recomendado

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

No usar como API principal:

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

Razon:

- `Then` se entiende como el siguiente paso.
- `ThenWith` se entiende como el siguiente paso usando valores adicionales.
- `ContinueIf` comunica guard de continuidad.
- `Branch` comunica division del flujo.
- `Return` y `End` son innecesarios si las reglas de `Then` y `RunAsync` ya expresan salida final.
- `Reject` no debe ser outcome generico porque rompe contratos de flows con `TResponse`.

## Flows Con Y Sin Respuesta

Debe haber dos familias.

```csharp
ComposeFlow<TRequest>(string name)
ComposeFlow<TRequest, TResponse>(string name)
```

Flow con respuesta:

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

Flow sin respuesta:

```csharp
await spider
    .ComposeFlow<CreateCustomerRequest>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .RunAsync(request, cancellationToken);
```

Reglas:

- `ComposeFlow<TRequest>` regresa `Task` o `ValueTask`.
- `ComposeFlow<TRequest, TResponse>` regresa `Task<TResponse>` o `ValueTask<TResponse>`.
- En flows sin respuesta, la firma activa final puede ser cualquier tipo.
- En flows con respuesta, la firma activa final debe ser `TResponse`.
- Si falta `TResponse`, debe fallar analyzer/build y runtime validation.

## Firma Activa E Historial

El runtime mantiene dos conceptos internos.

```txt
Firma activa:
  Valor que usa Then por default.

Historial:
  Valores iniciales y outputs previos.
  Solo se usan cuando el usuario lo declara con ThenWith.
```

Ejemplo:

```csharp
spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

Metodos:

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

Ejecucion:

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

`Then` ejecuta el siguiente step usando la firma activa.

Tambien se usa como primer step.

Firmas base:

```csharp
.Then(Func<TCurrent, TNext> step)
.Then(Func<TCurrent, CancellationToken, Task<TNext>> step)
.Then(Func<TCurrent, CancellationToken, Task> step)
.Then(Func<TCurrent, ValueTask<TNext>> step)
.Then(Func<TCurrent, CancellationToken, ValueTask<TNext>> step)
.Then(Func<TCurrent, CancellationToken, ValueTask> step)
```

Reglas:

- Si el step regresa valor, ese valor se vuelve firma activa.
- Si el step no regresa valor, la firma activa se conserva.
- Si lanza excepcion, el flow queda `faulted`.
- Si se cancela el token, el flow queda `cancelled`.

Validacion exception-based:

```csharp
.Then(Validate)
```

Si `Validate` lanza, falla el flow. Eso no requiere `ContinueIf`.

## ThenWith

`ThenWith` ejecuta el siguiente step usando valores concretos del historial.

```csharp
.ThenWith<CreateCustomerRequest, Customer>(Save)
```

Se lee:

```txt
Luego guarda con CreateCustomerRequest y Customer.
```

Firmas:

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

Reglas:

- Los tipos solicitados deben existir en el historial.
- Exact match gana sobre assignable match.
- Si hay multiples candidatos, es ambiguo.
- Si regresa valor, actualiza firma activa.
- Si no regresa valor, conserva firma activa.

## ContinueIf

`ContinueIf` es un guard de continuidad. Solo aplica a predicados booleanos.

No debe existir overload con `Action` o `Task`, porque eso ya es `Then(Validate)`.

Regla:

```txt
true  -> continua
false -> aplica otherwise
throw -> fault
```

Para flows con respuesta:

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

Tambien puede fallar con excepcion:

```csharp
.ContinueIf(
    HasValidEmail,
    otherwise: Flow.Throw(() => new ValidationException("Email is required")))
```

Para flows sin respuesta:

```csharp
await spider
    .ComposeFlow<SendWelcomeEmailCommand>("Send welcome email")
    .ContinueIf(
        ShouldSendWelcomeEmail,
        otherwise: Flow.Stop())
    .Then(SendEmail)
    .RunAsync(command, cancellationToken);
```

Outcomes permitidos:

```txt
Flow.Return(...)  -> solo flows con TResponse
Flow.Throw(...)   -> flows con o sin TResponse
Flow.Stop()       -> solo flows sin TResponse
```

No incluir `Flow.Reject` en v1.

Razon:

```txt
Reject sin TResponse rompe el contrato de ComposeFlow<TRequest, TResponse>.
Si el consumidor quiere un resultado semantico tipo rejected, debe modelarlo en TResponse.
```

Ejemplo:

```csharp
ComposeFlow<CreateCustomerRequest, CreateCustomerResult>
```

donde `CreateCustomerResult` puede representar success, validation failure o cualquier outcome de dominio.

## Branch

`Branch` divide el flow en ramas.

Ejemplo que converge:

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

Reglas:

- Cada branch recibe la firma activa actual.
- Cada rama puede usar `Then`, `ThenWith`, `ContinueIf` y `Branch`.
- Si hay steps despues de `Branch`, todas las ramas deben converger al mismo tipo activo.
- Si `Branch` es el ultimo elemento del flow sin respuesta, puede terminar con steps sin valor.
- Si `Branch` es el ultimo elemento del flow con respuesta, cada rama debe producir `TResponse` o lanzar excepcion.
- `Otherwise` debe ser obligatorio en v1 para evitar ramas no cubiertas.

Branch terminal sin respuesta:

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

Branch terminal con respuesta:

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

No hacen falta `Return` ni `End`. El contrato lo determina el tipo final esperado por el flow y la posicion del branch.

## Tipos Duplicados

Si dos steps producen el mismo tipo, `ThenWith<T>` puede ser ambiguo.

Ejemplo:

```csharp
.Then(LoadPrimaryCustomer)
.Then(LoadSecondaryCustomer)
.ThenWith<Customer>(ValidateCustomer)
```

Opciones:

1. Evitar duplicados del mismo tipo.
2. Crear tipos de dominio distintos.
3. Crear un record de estado.
4. Usar nombre como escape hatch.

Escape hatch:

```csharp
.Then(LoadPrimaryCustomer).NameOutput("primaryCustomer")
.Then(LoadSecondaryCustomer).NameOutput("secondaryCustomer")
.ThenWith<Customer>("primaryCustomer", ValidateCustomer)
```

No debe ser el happy path.

## Estado Explicito De Operacion

Cuando el flow necesita demasiados valores vivos, se debe crear un record de estado.

```csharp
public sealed record CreateCustomerState(
    CreateCustomerRequest Request,
    Customer Customer,
    CustomerPolicy Policy);
```

Uso:

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

Esto no es `Context` de Spider. Es estado de dominio controlado por la aplicacion.

## Aridad Soportada

Limite recomendado para v1:

```txt
ThenWith hasta 3 valores.
ComposeFlow hasta 3 inputs iniciales.
```

Si se requieren mas de 3 valores, usar estado explicito.

Razon:

```txt
La legibilidad cae rapido con aridad alta.
El objetivo es pseudocodigo simple, no un lenguaje de programacion alterno.
```

## Perfiles

Los perfiles son configuracion runtime reusable.

Ejemplo:

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

Uso:

```csharp
spider
    .ComposeFlow<CreateCustomerRequest, CustomerResponse>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .Then(ReturnResponse)
    .RunAsync(request, cancellationToken);
```

Responsabilidades de profile:

- telemetry on/off;
- metrics on/off;
- logging verbosity;
- redaction policy;
- boundaries;
- event enrichment;
- environment based behavior;
- feature flags.

Profiles no deben cambiar semantica de negocio por default.

## Arquitectura Runtime En Spider

Componentes propuestos dentro de Spider:

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

Agregar:

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

Los overloads de multiples inputs deben cuidarse para no explotar generics. V1 puede iniciar con 1 y 2 inputs.

### Flow Definition

Representa la estructura runtime:

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

Runtime bag interno:

```txt
entries:
  type
  optional name
  value
  source step id
```

No es publico.

### Step Invoker

Responsabilidades:

- resolver parametros desde active value o explicit history;
- inyectar `CancellationToken`;
- invocar delegates sync/async;
- clasificar retorno;
- actualizar active/history;
- preservar exception original.

## Analyzer Y Source Generator

Debe existir un proyecto de analyzer/source generator dentro de la solucion de Spider.

No debe ser necesario para ejecutar en runtime, pero si para generar metadata de KnOwl y diagnosticos tempranos.

Paquete candidato:

```txt
Spider.Pipelines.Analyzers
```

Nota:

```txt
ComposeFlow vive en Spider.
El analyzer puede empacarse como analyzer del mismo paquete NuGet o como paquete complementario si se decide despues.
La API runtime no debe depender del analyzer.
```

### Responsabilidades Del Analyzer

Validar:

- flow sin nombre;
- profile desconocido si es resoluble;
- `Then` incompatible con firma activa;
- `ThenWith` pide tipo no disponible;
- `ThenWith` ambiguo;
- final de flow con respuesta no produce `TResponse`;
- `ContinueIf` sin otherwise valido;
- `Flow.Stop()` usado en flow con `TResponse`;
- `Flow.Return(...)` incompatible con `TResponse`;
- branch sin `Otherwise`;
- branches que no convergen;
- lambda anonima con metadata limitada;
- aridad mayor al limite recomendado.

### Responsabilidades Del Source Generator

Generar descriptors estaticos para KnOwl.

Salida conceptual:

```csharp
internal static partial class SpiderGeneratedArchitecture
{
    public static ArchitectureManifest BuildManifest()
    {
        ...
    }
}
```

El generator no debe ejecutar codigo de usuario.

Debe usar:

- syntax tree;
- semantic model;
- method group symbols;
- generic arguments;
- attributes;
- source location.

## Metadata Para KnOwl

Spider debe publicar manifests usando el protocolo comun.

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

Cada step debe tener evidence cuando sea posible:

```txt
SourceKind: source-generator
ClrTypeName
MemberName
FilePath
LineNumber
DiscoveryMethod: source-generator
```

### Descriptor Ejemplo

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

## Telemetria Para RavenTracer

Spider debe emitir runtime events si el profile lo habilita.

Eventos:

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

Campos:

```txt
ComponentId = spider.flow:<flow-id> o spider.flow-step:<step-id>
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

Los payloads deben estar apagados por default.

Payloads, inputs y outputs deben exponerse solo via redaction policy/profile.

## Fluent API Recomendada

### Caso Simple Con Respuesta

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

### Usando Valores Previos

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

### Sin Respuesta

```csharp
await spider
    .ComposeFlow<CreateCustomerRequest>("Create customer")
    .UsingProfile("Business")
    .Then(Validate)
    .Then(Map)
    .ThenWith<CreateCustomerRequest, Customer>(Save)
    .RunAsync(request, cancellationToken);
```

### ContinueIf Con Response Temprano

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

### ContinueIf Con Excepcion

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

### ContinueIf En Flow Sin Respuesta

```csharp
await spider
    .ComposeFlow<SendWelcomeEmailCommand>("Send welcome email")
    .ContinueIf(
        ShouldSendEmail,
        otherwise: Flow.Stop())
    .Then(SendEmail)
    .RunAsync(command, cancellationToken);
```

### Branch Que Converge

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

### Branch Terminal Con Respuesta

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

### Branch Terminal Sin Respuesta

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

### Estado Explicito

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

## Plan Para Generar Metadata

La metadata completa para documentacion debe generarse en build time. Runtime queda reservado para telemetria y para el manifiesto de composicion disponible hoy.

### Estado Implementado

Spider ya publica un manifest de arquitectura desde runtime composition:

- `spider.pipeline` para pipelines configurados con `Attach`;
- `spider.pipeline-stage` para preprocess, middleware, target, parallel, success postprocess y failure postprocess;
- `spider.flow` para flows compuestos con `ComposeFlow`;
- `spider.flow-step`, `spider.flow-condition` y `spider.flow-branch` para pasos de flow;
- `spider.flow-profile` para perfiles usados con `UsingProfile`;
- relaciones `contains`, `next` y `uses-profile`;
- `ISpiderArchitectureProvider.GetManifest()` como API publica de lectura.

Este corte permite integracion temprana con KnOwl para mapas de proceso/pseudocodigo sin esperar al analyzer. No incluye evidence source-level ni diagnosticos de compilacion.

La salida de metadata debe poder alimentar KnOwl sin que KnOwl conozca internals de Spider. Spider actua como producer semantico y publica un manifest compatible con el protocolo de arquitectura.

### Objetivo Del Primer Corte

Generar un manifest estatico con:

- flows declarados con `ComposeFlow`;
- steps agregados con `Then`;
- steps agregados con `ThenWith`;
- conditions agregadas con `ContinueIf`;
- branches agregados con `Branch`, `When` y `Otherwise`;
- profiles usados con `UsingProfile`;
- evidence de archivo, linea, tipo CLR y metodo;
- diagnostics cuando el flow no puede documentarse bien.

No intentar generar telemetria desde el source generator. Eso pertenece a RavenTracer/runtime.

### Proyecto Sugerido

Crear un proyecto analyzer/source generator:

```txt
src/Spider.Pipelines.Analyzers
```

Este proyecto debe empacarse como analyzer del paquete `Spider.Pipelines` o como paquete complementario. Decision recomendada para v1:

```txt
Empacarlo dentro de Spider.Pipelines como analyzer incluido.
```

Razon:

- el usuario obtiene diagnostics sin instalar otro paquete;
- el manifest se genera automaticamente;
- la funcionalidad se siente parte de Spider;
- no agrega dependencias runtime al paquete principal.

El runtime de Spider no debe depender del analyzer.

### Contratos De Metadata

Para no bloquearse esperando los paquetes finales de Elysium Architecture, Spider puede iniciar con contratos internos estables y mapearlos despues.

Namespace sugerido:

```txt
Spider.Pipelines.Architecture
```

Tipos internos o publicos iniciales:

```csharp
SpiderArchitectureManifest
SpiderComponentDescriptor
SpiderOperationDescriptor
SpiderRelationDescriptor
SpiderEvidenceDescriptor
SpiderDiagnosticDescriptor
SpiderMetadataBag
```

Cuando existan:

```txt
Elysium.Architecture.Abstractions
```

se agrega adapter:

```txt
SpiderArchitectureManifest -> ArchitectureManifest
```

No acoplar la primera version del runtime a KnOwl directamente.

### IDs Estables

El source generator debe crear IDs logicos. El CLR es evidencia, no identidad primaria.

Formato recomendado:

```txt
spider.flow:{normalized-flow-name}
spider.flow-step:{normalized-flow-name}.{step-name-or-index}
spider.flow-branch:{normalized-flow-name}.{branch-index}
spider.flow-condition:{normalized-flow-name}.{condition-name-or-index}
spider.flow-profile:{profile-name}
```

Ejemplo:

```txt
spider.flow:create-customer
spider.flow-step:create-customer.validate
spider.flow-step:create-customer.map
spider.flow-step:create-customer.save
spider.flow-step:create-customer.return-response
spider.flow-profile:business
```

Si el mismo nombre aparece mas de una vez en el mismo scope, agregar sufijo estable por posicion:

```txt
spider.flow-step:create-customer.validate-2
```

No usar line number como parte del ID, porque rompe estabilidad con cambios de formato. Line number solo va en evidence.

### Evidence

Cada descriptor debe incluir evidence cuando sea posible:

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

Para method groups:

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

Para lambdas:

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

Ejemplo conceptual:

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

El generator debe emitir un archivo parecido a:

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

Tambien puede emitir descriptors por assembly:

```csharp
internal static partial class SpiderGeneratedArchitecture_AssemblyName
```

El host de KnOwl o el provider de Spider puede buscar estos tipos por reflection.

### Provider Para KnOwl

Crear un provider en Spider:

```csharp
public interface ISpiderArchitectureProvider
{
    SpiderArchitectureManifest GetManifest();
}
```

Implementacion:

```csharp
GeneratedSpiderArchitectureProvider
```

Responsabilidad:

- cargar manifest generado;
- agregar metadata runtime/config si aplica;
- exponerlo a KnOwl adapter.

Cuando existan abstracciones comunes:

```csharp
public sealed class SpiderArchitectureProvider : IArchitectureManifestProvider
```

### Analyzer Diagnostics

Diagnostics recomendados:

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

Severity inicial:

- errores de contrato: error;
- metadata incompleta: warning;
- perfil desconocido: warning al principio, error si se habilita politica estricta.

### Como Analizar La Fluent API

El analyzer debe encontrar invocations a:

```csharp
ComposeFlow(...)
```

Despues debe caminar la cadena fluent:

```txt
ComposeFlow
  -> UsingProfile?
  -> Then*
  -> ThenWith*
  -> ContinueIf*
  -> Branch*
  -> RunAsync
```

Debe resolver semanticamente:

- generic arguments;
- receiver type despues de cada metodo;
- method group symbol;
- lambda syntax;
- return type;
- CancellationToken parameter;
- branch route chains.

No debe ejecutar codigo.

### Estrategia Por Fases

#### Metadata Fase 1: Lineal

Soportar:

- `ComposeFlow<TRequest>`;
- `ComposeFlow<TRequest, TResponse>`;
- `UsingProfile`;
- `Then`;
- `ThenWith`;
- `RunAsync`.

Generar:

- flow component;
- step components;
- contains relations;
- next relations;
- uses-profile relation;
- input/output metadata.

#### Metadata Fase 2: ContinueIf

Soportar:

- `ContinueIf(..., Flow.Return(...))`;
- `ContinueIf(..., Flow.Throw(...))`;
- `ContinueIf(..., Flow.Stop())`.

Generar:

- condition component;
- relation `next` hacia condition;
- relation `returns` o `throws`;
- metadata de early exit.

#### Metadata Fase 3: Branch

Soportar:

- `Branch`;
- `When`;
- `Otherwise`;
- nested route chains lineales.

Generar:

- branch component;
- condition components;
- `branches-to`;
- `otherwise`;
- convergence metadata.

#### Metadata Fase 4: Nested Flows

Detectar si un step llama otro metodo que contiene `ComposeFlow`.

Primera version:

- solo cuando el source generator puede ver el metodo llamado en el mismo compilation.

Generar:

```txt
calls-flow
```

Si no puede resolver:

```txt
unresolved reference warning
```

#### Metadata Fase 5: Elysium Protocol Adapter

Mapear:

```txt
SpiderArchitectureManifest -> ArchitectureManifest
SpiderComponentDescriptor -> ComponentDescriptor
SpiderRelationDescriptor -> RelationDescriptor
SpiderEvidenceDescriptor -> EvidenceDescriptor
```

Exponer provider compatible con KnOwl.

### Criterios De Aceptacion

Primer corte listo cuando:

- un proyecto consumidor que usa `ComposeFlow` genera manifest en build;
- el manifest contiene flow, steps y relations;
- los method groups tienen evidence CLR/source;
- lambdas generan warning de metadata limitada;
- `ThenWith` invalido produce diagnostic;
- flow con `TResponse` insatisfecho produce diagnostic;
- se puede leer el manifest desde tests sin ejecutar el flow;
- el runtime sigue funcionando sin el analyzer.

### Pruebas Necesarias

Crear tests de analyzer/source generator con `Microsoft.CodeAnalysis.CSharp.Testing`.

Casos:

- flow lineal con respuesta genera manifest correcto;
- flow sin respuesta genera manifest correcto;
- `ThenWith` genera uses metadata;
- `UsingProfile` genera relation `uses-profile`;
- lambda genera warning;
- branch genera branch descriptors;
- flow invalido genera diagnostic;
- generated manifest compila.

## Orden De Implementacion

### Fase 1: API Y Runtime Basico

- Agregar `ComposeFlow<TRequest>` y `ComposeFlow<TRequest, TResponse>` a `ISpider`.
- Implementar `Then`.
- Implementar `ThenWith` hasta aridad 2.
- Implementar flows con y sin respuesta.
- Implementar `RunAsync`.
- Validar output final en flows con respuesta.
- Agregar tests de sync/async/value/no-value.

### Fase 2: Guards De Continuidad

- Implementar `ContinueIf`.
- Implementar `Flow.Return`.
- Implementar `Flow.Throw`.
- Implementar `Flow.Stop` solo para flows sin respuesta.
- Validar errores de contrato.

### Fase 3: Branches

- Implementar `Branch`, `When`, `Otherwise`.
- Validar convergencia.
- Validar branch terminal con/sin respuesta.
- Agregar tests de branch nested basicos.

### Fase 4: Profiles

- Agregar `AddFlowProfile`.
- Agregar `UsingProfile`.
- Agregar options de telemetry/metrics/logging/redaction/boundaries.
- Mantener semantica de negocio sin cambios.

### Fase 5: Metadata Interna

- Crear descriptors runtime de flow/step/branch/condition.
- Crear ids estables.
- Agregar evidence basica.
- Preparar shape compatible con KnOwl.

### Fase 6: Analyzer/Source Generator

- Implementar diagnostics.
- Generar manifest estatico.
- Generar ids/relations/evidence.
- Exponer extension para que KnOwl ingiera manifests.

### Fase 7: Telemetry Adapter

- Emitir runtime events.
- Correlacionar flows anidados.
- Correlacionar flow dentro de pipeline.
- Exponer adapter para RavenTracer.

### Fase 8: Integracion KnOwl/RavenTracer

- Crear `SpiderArchitectureProvider`.
- Crear `SpiderTelemetryAdapter`.
- Probar primer vertical:

```txt
Pigeon Consumer
  -> Spider Pipeline
  -> Handler
  -> Spider ComposeFlow
  -> Validator/Mapper/Outbox
```

## Decisiones Cerradas Para V1

- `ComposeFlow` vive dentro de Spider.
- `Then` es el step normal.
- `ThenWith` usa valores previos declarados.
- `ContinueIf` solo acepta predicados booleanos.
- Validaciones exception-based se modelan con `Then`.
- No existe `Reject` como outcome core.
- Flow con respuesta solo puede terminar con `TResponse` o exception.
- Flow sin respuesta puede terminar con `Flow.Stop`.
- Branch debe tener `Otherwise` en v1.
- Branch debe converger o ser terminal.
- Runtime no genera documentacion.
- Source generator/analyzer genera metadata para KnOwl.
- Runtime events alimentan RavenTracer.

## Riesgos

### Inference De C#

Algunas combinaciones de method groups y generics pueden requerir overloads especificos.

Mitigacion:

- empezar con aridad 1 y 2;
- agregar helpers solo cuando haya casos reales;
- preferir errores claros sobre magia.

### Ambiguedad De Historial

Mismo tipo producido mas de una vez.

Mitigacion:

- error por default;
- recomendar records de estado;
- `NameOutput` como escape hatch.

### Source Generator Complejo

Analizar cadenas fluent con branches puede crecer rapido.

Mitigacion:

- fasear generator despues del runtime;
- soportar primero shape lineal;
- agregar branches despues.

### Demasiado Poder En Branch

Puede convertirse en lenguaje alterno.

Mitigacion:

- no joins complejos en v1;
- no grafo arbitrario;
- branch converge o termina.

## Resultado Esperado

Con `ComposeFlow`, Spider podra publicar a KnOwl algo como:

```txt
spider.flow:create-customer
  contains Validate
  next Map
  next Save
  next ReturnResponse
  uses-profile Business
```

Y RavenTracer podra mostrar:

```txt
Create customer started
  Validate completed
  Map completed
  Save faulted
Create customer faulted
```

Con correlacion:

```txt
Definition: spider.flow:create-customer
Runtime: trace 7f3...
Failure: spider.flow-step:create-customer.save
Evidence: CreateCustomerService.Save, file/line
```

Ese es el punto real: no solo encadenar lambdas, sino convertir procesos de negocio locales en arquitectura consultable y ejecucion observable.
