using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Testing;
using Spider.Testing.Assertions;
using System.Reflection;

namespace Spider.Testing.Tests
{
    /// <summary>
    /// Verifies the Spider testing package behavior.
    /// </summary>
    public sealed class SpiderTestingTests
    {
        /// <summary>
        /// Verifies that Spider testing registers discovered components and executes a request.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_ShouldExecuteDiscoveredHandlerAndExposeTrace()
        {
            var spider = CreateSpider();

            var result = await spider.ExecuteAsync<CustomerResult>(new CustomerCommand("C-100"));
            var trace = spider.Trace;

            Assert.Equal("handled:C-100", result.Value);
            trace.ShouldContain("TransactionBoundary");
            trace.ShouldContain("ExceptionBoundary");
            trace.ShouldContain("HandlerExecution");
            trace.ShouldRunBefore("TransactionBoundary", "HandlerExecution");
            trace.ShouldRecordRequest<CustomerCommand>();
            trace.Transaction.ShouldBegin();
            trace.Transaction.ShouldCommit();
        }

        /// <summary>
        /// Verifies that handler failures roll back transaction boundaries.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task ExecuteAsync_WhenHandlerFails_ShouldRollbackTransactionTrace()
        {
            var spider = CreateSpider();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => spider.ExecuteAsync<CustomerResult>(new FailingCustomerCommand("C-500")));

            spider.Trace.ShouldContain("HandlerExecution:Fault");
            spider.Trace.Transaction.ShouldBegin();
            spider.Trace.Transaction.ShouldRollback();
        }

        /// <summary>
        /// Verifies that a test can simulate a failure inside a specific boundary.
        /// </summary>
        /// <returns>A task representing the asynchronous test.</returns>
        [Fact]
        public async Task FailInside_ShouldShortCircuitBeforeHandler()
        {
            var spider = CreateSpider();
            spider.FailInside<ExceptionBoundary>(new InvalidOperationException("Boundary failed."));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => spider.ExecuteAsync<CustomerResult>(new CustomerCommand("C-101")));

            Assert.Equal("Boundary failed.", exception.Message);
            spider.Trace.ShouldContain("ExceptionBoundary");
            spider.Trace.ShouldNotContain("HandlerExecution");
        }

        [Fact]
        public async Task ExecuteAsync_WhenHandlerCancels_ShouldRecordCancellationAndRollbackTransaction()
        {
            var spider = CreateSpider();

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => spider.ExecuteAsync<object>(new CancellingCommand("C-408")));

            spider.Trace.ShouldContain("PipelineExecution:Cancel");
            spider.Trace.Transaction.ShouldBegin();
            spider.Trace.Transaction.ShouldRollback();
        }

        [Fact]
        public async Task ExecuteAsync_WhenHandlerReturnsSynchronousResult_ShouldReturnResult()
        {
            var spider = CreateSpider();

            var result = await spider.ExecuteAsync<CustomerResult>(new SyncCustomerCommand("C-200"));

            Assert.Equal("sync:C-200", result.Value);
            spider.Trace.ShouldContain("HandlerExecution:Complete");
        }

        [Fact]
        public async Task ExecuteAsync_WhenHandlerReturnsGenericValueTask_ShouldReturnResult()
        {
            var spider = CreateSpider();

            var result = await spider.ExecuteAsync<CustomerResult>(new ValueTaskCustomerCommand("C-201"));

            Assert.Equal("value-task:C-201", result.Value);
            spider.Trace.ShouldContain("HandlerExecution:Complete");
        }

        [Fact]
        public async Task ExecuteAsync_WhenHandlerReturnsVoidValueTask_ShouldReturnDefaultResponse()
        {
            var spider = CreateSpider();

            var result = await spider.ExecuteAsync<object>(new VoidValueTaskCommand("C-202"));

            Assert.Null(result);
            spider.Trace.ShouldContain("HandlerExecution:Complete");
        }

        [Fact]
        public async Task ExecuteAsync_WhenHandlerReturnsVoidOrTask_ShouldReturnDefaultResponse()
        {
            var spider = CreateSpider();

            var voidResult = await spider.ExecuteAsync<object>(new VoidCustomerCommand("C-203"));
            var taskResult = await spider.ExecuteAsync<object>(new TaskOnlyCustomerCommand("C-204"));
            var delayTaskResult = await spider.ExecuteAsync<object>(new DelayTaskCustomerCommand("C-205"));

            Assert.Null(voidResult);
            Assert.Equal("VoidTaskResult", taskResult.GetType().Name);
            Assert.Null(delayTaskResult);
        }

        [Fact]
        public async Task ExecuteAsync_WhenRequestIsNull_ShouldThrow()
        {
            var spider = CreateSpider();

            await Assert.ThrowsAsync<ArgumentNullException>(() => spider.ExecuteAsync(null));
        }

        [Fact]
        public async Task ExecuteAsync_WhenHandlerIsMissing_ShouldThrow()
        {
            var spider = CreateSpider();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => spider.ExecuteAsync<object>(new MissingCustomerCommand("C-404")));

            Assert.Contains("No Spider testing handler", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenMultipleHandlersMatchExactly_ShouldThrow()
        {
            var spider = CreateSpider();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => spider.ExecuteAsync<CustomerResult>(new DuplicateCustomerCommand("C-409")));

            Assert.Contains("Multiple Spider testing handlers", exception.Message);
        }

        [Fact]
        public void AddSpiderTesting_WhenServicesIsNull_ShouldThrow()
        {
            IServiceCollection services = null;

            Assert.Throws<ArgumentNullException>(() => services.AddSpiderTesting(typeof(CustomerCommand).Assembly));
        }

        [Fact]
        public void AddSpiderTesting_WhenAssembliesAreMissing_ShouldThrow()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() => services.AddSpiderTesting());
        }

        [Fact]
        public void FailInside_WhenExceptionIsNull_ShouldThrow()
        {
            var spider = CreateSpider();

            Assert.Throws<ArgumentNullException>(() => spider.FailInside<ExceptionBoundary>(null));
        }

        [Fact]
        public void TraceAssertions_WhenExpectationFails_ShouldThrowHelpfulException()
        {
            var trace = new ExecutionTrace();
            trace.Add("First", "Begin", typeof(CustomerCommand));
            trace.Add("Second", "Complete", typeof(CustomerCommand));

            Assert.Throws<ArgumentNullException>(() => ExecutionTraceAssertions.ShouldContain(null, "First"));
            Assert.Throws<ArgumentException>(() => trace.ShouldContain(""));
            Assert.Throws<ArgumentNullException>(() => ExecutionTraceAssertions.ShouldNotContain(null, "First"));
            Assert.Throws<ArgumentException>(() => trace.ShouldNotContain(""));
            Assert.Throws<ArgumentNullException>(() => ExecutionTraceAssertions.ShouldRunBefore(null, "First", "Second"));
            Assert.Throws<ArgumentNullException>(() => ExecutionTraceAssertions.ShouldRecordRequest<CustomerCommand>(null));
            Assert.Throws<SpiderTraceAssertionException>(() => trace.ShouldContain("Missing"));
            Assert.Throws<SpiderTraceAssertionException>(() => trace.ShouldNotContain("First"));
            Assert.Throws<SpiderTraceAssertionException>(() => trace.ShouldRunBefore("Second", "First"));
            Assert.Throws<SpiderTraceAssertionException>(() => trace.ShouldRunBefore("Missing", "First"));
            Assert.Throws<SpiderTraceAssertionException>(() => trace.ShouldRunBefore("First", "Missing"));
            Assert.Throws<SpiderTraceAssertionException>(() => new ExecutionTrace().ShouldRecordRequest<CustomerCommand>());
        }

        [Fact]
        public void TransactionAssertions_WhenExpectationFails_ShouldThrowHelpfulException()
        {
            var trace = new ExecutionTrace();
            trace.Add("CustomBoundary", "Begin", typeof(CustomerCommand));

            Assert.Single(trace.ForTransaction("CustomBoundary").Events);
            Assert.Empty(trace.ForTransaction("OtherBoundary").Events);
            Assert.Throws<ArgumentNullException>(() => TransactionTraceAssertions.ShouldBegin(null));
            Assert.Throws<SpiderTraceAssertionException>(() => trace.Transaction.ShouldCommit());
        }

        [Fact]
        public void SpiderTestingOptions_ShouldRejectInvalidAssemblies()
        {
            var options = new SpiderTestingOptions();

            Assert.Throws<ArgumentNullException>(() => options.AddAssemblies(null));
            Assert.Throws<ArgumentException>(() => options.AddAssemblies(new Assembly[] { null }));

            options.AddAssemblies(new[] { typeof(CustomerCommand).Assembly, typeof(CustomerCommand).Assembly });

            Assert.Single(options.Assemblies);
        }

        [Fact]
        public void SpiderTestingBuilder_ShouldExposeServices()
        {
            var services = new ServiceCollection();
            var builder = new SpiderTestingBuilder(services);

            Assert.Same(services, builder.Services);
            Assert.Throws<ArgumentNullException>(() => new SpiderTestingBuilder(null));
        }

        [Fact]
        public void ExecutionTraceEvent_ShouldValidateRequiredValuesAndExposeDefaults()
        {
            Assert.Throws<ArgumentNullException>(() => new ExecutionTraceEvent(null, "Begin", typeof(CustomerCommand), null, null));
            Assert.Throws<ArgumentNullException>(() => new ExecutionTraceEvent("Event", null, typeof(CustomerCommand), null, null));

            var traceEvent = new ExecutionTraceEvent("Event", "Begin", typeof(CustomerCommand), null, null);

            Assert.Equal("Event", traceEvent.Name);
            Assert.Equal("Begin", traceEvent.Operation);
            Assert.Equal(typeof(CustomerCommand), traceEvent.RequestType);
            Assert.Null(traceEvent.Exception);
            Assert.Empty(traceEvent.Metadata);
            Assert.Equal("Event:Begin", traceEvent.ToString());
        }

        [Fact]
        public void InternalDescriptors_ShouldValidateConstructorArguments()
        {
            var boundaryDescriptorType = typeof(ISpiderTesting).Assembly.GetType("Spider.Testing.Internals.BoundaryDescriptor");
            var handlerDescriptorType = typeof(ISpiderTesting).Assembly.GetType("Spider.Testing.Internals.HandlerDescriptor");
            var handleMethod = typeof(CustomerHandler).GetMethod(nameof(CustomerHandler.Handle), new[] { typeof(SyncCustomerCommand) });

            Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(boundaryDescriptorType, new object[] { null }));
            Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(boundaryDescriptorType, typeof(CustomerHandler)));

            var descriptor = Activator.CreateInstance(
                handlerDescriptorType,
                typeof(CustomerHandler),
                handleMethod,
                typeof(SyncCustomerCommand),
                typeof(CustomerResult));

            Assert.NotNull(descriptor);
            Assert.True((bool)handlerDescriptorType.GetProperty("HasResponse").GetValue(descriptor));
            Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(handlerDescriptorType, null, handleMethod, typeof(SyncCustomerCommand), typeof(CustomerResult)));
            Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(handlerDescriptorType, typeof(CustomerHandler), null, typeof(SyncCustomerCommand), typeof(CustomerResult)));
            Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(handlerDescriptorType, typeof(CustomerHandler), handleMethod, null, typeof(CustomerResult)));
        }

        [Fact]
        public void Discovery_ShouldHandlePartiallyLoadableAssemblies()
        {
            var discoveryType = typeof(ISpiderTesting).Assembly.GetType("Spider.Testing.Internals.SpiderTestingDiscovery");
            var method = discoveryType.GetMethod("Discover", BindingFlags.Public | BindingFlags.Static);

            Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { null }));

            var registry = method.Invoke(null, new object[] { new[] { new PartiallyLoadableAssembly() } });

            Assert.NotNull(registry);

            var isHandlerMethod = discoveryType.GetMethod("IsHandlerMethod", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.False((bool)isHandlerMethod.Invoke(null, new object[] { null }));
        }

        [Fact]
        public async Task InternalHandlerInvoker_ShouldValidateRequiredArguments()
        {
            var invokerType = typeof(ISpiderTesting).Assembly.GetType("Spider.Testing.Internals.HandlerInvoker");
            var method = invokerType.GetMethod("InvokeAsync", BindingFlags.Public | BindingFlags.Static);
            var handleMethod = typeof(CustomerHandler).GetMethod(nameof(CustomerHandler.Handle), new[] { typeof(SyncCustomerCommand) });
            var handlerDescriptorType = typeof(ISpiderTesting).Assembly.GetType("Spider.Testing.Internals.HandlerDescriptor");
            var descriptor = Activator.CreateInstance(
                handlerDescriptorType,
                typeof(CustomerHandler),
                handleMethod,
                typeof(SyncCustomerCommand),
                typeof(CustomerResult));

            await Assert.ThrowsAsync<ArgumentNullException>(() => InvokeReflectedTask(method, null, new CustomerHandler(), new SyncCustomerCommand("C-205")));
            await Assert.ThrowsAsync<ArgumentNullException>(() => InvokeReflectedTask(method, descriptor, null, new SyncCustomerCommand("C-205")));
        }

        private static ISpiderTesting CreateSpider()
        {
            var services = new ServiceCollection();
            services.AddSpiderTesting(typeof(CustomerCommand).Assembly);
            return services.BuildServiceProvider().GetRequiredService<ISpiderTesting>();
        }

        private static async Task<object> InvokeReflectedTask(MethodInfo method, object descriptor, object service, object request)
        {
            var task = (Task<object>)method.Invoke(null, new[] { descriptor, service, request, CancellationToken.None });
            return await task;
        }
    }

    /// <summary>
    /// Provides a sample request.
    /// </summary>
    public sealed record CustomerCommand(string Id);

    /// <summary>
    /// Provides a sample failing request.
    /// </summary>
    public sealed record FailingCustomerCommand(string Id);

    public sealed record CancellingCommand(string Id);

    public sealed record SyncCustomerCommand(string Id);

    public sealed record ValueTaskCustomerCommand(string Id);

    public sealed record VoidValueTaskCommand(string Id);

    public sealed record VoidCustomerCommand(string Id);

    public sealed record TaskOnlyCustomerCommand(string Id);

    public sealed record DelayTaskCustomerCommand(string Id);

    public sealed record MissingCustomerCommand(string Id);

    public sealed record DuplicateCustomerCommand(string Id);

    public sealed record InvalidHandlerCommand(string Id);

    /// <summary>
    /// Provides a sample response.
    /// </summary>
    public sealed record CustomerResult(string Value);

    /// <summary>
    /// Provides a sample transaction boundary.
    /// </summary>
    public sealed class TransactionBoundary : PipelineExecutionBoundary
    {
    }

    /// <summary>
    /// Provides a sample exception boundary.
    /// </summary>
    public sealed class ExceptionBoundary : PipelineExecutionBoundary
    {
    }

    /// <summary>
    /// Provides a sample customer handler.
    /// </summary>
    public sealed class CustomerHandler
    {
        /// <summary>
        /// Handles a customer command.
        /// </summary>
        /// <param name="command">The customer command.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The customer result.</returns>
        public Task<CustomerResult> HandleAsync(CustomerCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new CustomerResult($"handled:{command.Id}"));

        /// <summary>
        /// Handles a failing customer command.
        /// </summary>
        /// <param name="command">The failing customer command.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The customer result.</returns>
        public Task<CustomerResult> HandleAsync(FailingCustomerCommand command, CancellationToken cancellationToken)
            => throw new InvalidOperationException($"failed:{command.Id}");

        public Task<CustomerResult> HandleAsync(CancellingCommand command, CancellationToken cancellationToken)
            => throw new OperationCanceledException(cancellationToken);

        public CustomerResult Handle(SyncCustomerCommand command)
            => new($"sync:{command.Id}");

        public ValueTask<CustomerResult> HandleAsync(ValueTaskCustomerCommand command)
            => ValueTask.FromResult(new CustomerResult($"value-task:{command.Id}"));

        public ValueTask HandleAsync(VoidValueTaskCommand command)
            => ValueTask.CompletedTask;

        public void Handle(VoidCustomerCommand command)
        {
        }

        public Task HandleAsync(TaskOnlyCustomerCommand command)
            => Task.CompletedTask;

        public Task HandleAsync(DelayTaskCustomerCommand command)
            => Task.Delay(1);
    }

    public sealed class DuplicateCustomerHandlerA
    {
        public Task<CustomerResult> HandleAsync(DuplicateCustomerCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new CustomerResult(command.Id));
    }

    public sealed class DuplicateCustomerHandlerB
    {
        public Task<CustomerResult> HandleAsync(DuplicateCustomerCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new CustomerResult(command.Id));
    }

    public sealed class InvalidPropertyHandlerShape
    {
        public CustomerResult Handle => new("property");
    }

    public sealed class InvalidEmptyHandlerShape
    {
        public void Handle()
        {
        }
    }

    public sealed class InvalidSecondParameterHandlerShape
    {
        public void Handle(InvalidHandlerCommand command, string invalidSecondParameter)
        {
        }
    }

    public sealed class InvalidTooManyParametersHandlerShape
    {
        public void Handle(InvalidHandlerCommand command, string one, string two)
        {
        }
    }

    public sealed class InvalidCancellationTokenRequestHandlerShape
    {
        public void Handle(CancellationToken cancellationToken)
        {
        }
    }

    public sealed class PartiallyLoadableAssembly : Assembly
    {
        public override Type[] GetTypes()
            => throw new ReflectionTypeLoadException(
                new[] { typeof(CustomerCommand), null },
                new Exception[] { new InvalidOperationException("Cannot load one type.") });
    }

}
