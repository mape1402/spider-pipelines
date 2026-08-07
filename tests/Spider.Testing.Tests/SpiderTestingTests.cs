using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Testing;
using Spider.Testing.Assertions;

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

        private static ISpiderTesting CreateSpider()
        {
            var services = new ServiceCollection();
            services.AddSpiderTesting(typeof(CustomerCommand).Assembly);
            return services.BuildServiceProvider().GetRequiredService<ISpiderTesting>();
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
    }
}
