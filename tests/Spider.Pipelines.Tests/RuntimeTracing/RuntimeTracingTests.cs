using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;
using Spider.Pipelines.RuntimeTracing;
using Spider.Pipelines.RuntimeTracing.Stores;

namespace Spider.Pipelines.Tests.RuntimeTracing
{
    public sealed class RuntimeTracingTests
    {
        [Fact]
        public void AddSpiderRuntimeTracing_WhenNoStoreIsConfigured_ShouldUseInMemoryStore()
        {
            var services = new ServiceCollection();

            services.AddSpider();
            services.AddSpiderRuntimeTracing();

            using var provider = services.BuildServiceProvider();

            Assert.IsType<InMemorySpiderTraceStore>(provider.GetRequiredService<ISpiderTraceStore>());
            Assert.Same(
                provider.GetRequiredService<ISpiderTraceStore>(),
                provider.GetRequiredService<ISpiderTraceReader>());
            Assert.True(provider.GetRequiredService<ISpiderRuntimeTracer>().IsEnabled);
        }

        [Fact]
        public void AddSpiderRuntimeTracing_WhenCustomStoreIsConfigured_ShouldUseCustomStore()
        {
            var services = new ServiceCollection();

            services.AddSpider();
            services.AddSpiderRuntimeTracing(tracing => tracing.UseStore<RecordingTraceStore>());

            using var provider = services.BuildServiceProvider();

            Assert.IsType<RecordingTraceStore>(provider.GetRequiredService<ISpiderTraceStore>());
            Assert.Same(
                provider.GetRequiredService<ISpiderTraceStore>(),
                provider.GetRequiredService<ISpiderTraceWriter>());
        }

        [Fact]
        public async Task ComposeFlow_WhenRuntimeTracingIsEnabled_ShouldEmitFlowAndStepEvents()
        {
            var services = new ServiceCollection();
            services.AddSpider();
            services.AddSpiderRuntimeTracing();
            using var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();

            var response = await spider
                .ComposeFlow<CustomerCommand, CustomerResult>("Create customer")
                .Then(ValidateCustomer)
                .Then(MapCustomer)
                .RunAsync(new CustomerCommand("ada@example.com"), CancellationToken.None);

            await provider.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync();

            var trace = await GetOnlyTraceAsync(provider);

            Assert.Equal("ada@example.com", response.Email);
            Assert.All(trace.Events, item => Assert.Equal(trace.TraceId, item.TraceId));
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.FlowStarted);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.FlowStepStarted && item.DisplayName == nameof(ValidateCustomer) && !string.IsNullOrWhiteSpace(item.ParentSpanId));
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.FlowStepCompleted);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.FlowCompleted);
        }

        [Fact]
        public async Task Pipeline_WhenRuntimeTracingIsEnabled_ShouldEmitPipelineStageAndBoundaryEvents()
        {
            var services = new ServiceCollection();
            services.AddSingleton<CustomerService>();
            services.AddScoped<RecordingBoundary>();
            services.AddSpider(builder => builder.AddExecutionBoundary<RecordingBoundary>());
            services.AddSpiderRuntimeTracing();
            using var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();

            var result = await spider
                .InitBridge<CustomerService>()
                .Attach<CustomerCommand, CustomerResult>(builder => builder
                    .PreProcess((ctx, args) => Task.CompletedTask)
                    .OnSuccess((ctx, args) => Task.CompletedTask))
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), new CustomerCommand("ada@example.com"));

            await provider.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync();

            var trace = await GetOnlyTraceAsync(provider);

            Assert.Equal("ada@example.com", result.Email);
            Assert.All(trace.Events, item => Assert.Equal(trace.TraceId, item.TraceId));
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStarted);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineStageStarted && item.DisplayName == "Pre-process");
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.BoundaryBeginStarted && item.DisplayName == nameof(RecordingBoundary) && !string.IsNullOrWhiteSpace(item.ParentSpanId));
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.BoundaryCompleteStarted && item.DisplayName == nameof(RecordingBoundary));
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineCompleted);
        }

        [Fact]
        public async Task Pipeline_WhenTargetFails_ShouldEmitFaultedTraceWithoutReplacingOriginalException()
        {
            var services = new ServiceCollection();
            services.AddSingleton<CustomerService>();
            services.AddScoped<RecordingBoundary>();
            services.AddSpider(builder => builder.AddExecutionBoundary<RecordingBoundary>());
            services.AddSpiderRuntimeTracing();
            using var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => spider
                .InitBridge<CustomerService>()
                .Attach<CustomerCommand, CustomerResult>(builder => builder
                    .OnFailure((ctx, args) => Task.CompletedTask))
                .ExecuteAsync(service => (request, token) => service.FailAsync(request, token), new CustomerCommand("ada@example.com")));

            await provider.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync();

            var trace = await GetOnlyTraceAsync(provider);

            Assert.Equal("Customer failed.", exception.Message);
            Assert.Equal(SpiderTraceStatus.Faulted, trace.Status);
            Assert.All(trace.Events, item => Assert.Equal(trace.TraceId, item.TraceId));
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineFaulted);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.BoundaryFaultStarted);
            Assert.Contains(trace.Events, item => item.Exception != null && item.Exception.Message == "Customer failed.");
        }

        [Fact]
        public async Task ComposeFlow_WhenRuntimeTracingIsOff_ShouldNotStoreEvents()
        {
            var services = new ServiceCollection();
            services.AddSpider();
            services.AddSpiderRuntimeTracing(tracing => tracing.Verbosity = SpiderTraceVerbosity.Off);
            using var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();

            var response = await spider
                .ComposeFlow<CustomerCommand, CustomerResult>("Create customer")
                .Then(ValidateCustomer)
                .Then(MapCustomer)
                .RunAsync(new CustomerCommand("ada@example.com"), CancellationToken.None);

            await provider.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync();

            var traces = await GetTracesAsync(provider);

            Assert.Equal("ada@example.com", response.Email);
            Assert.Empty(traces);
        }

        [Fact]
        public async Task Pipeline_WhenTraceSinkFails_ShouldNotBreakBusinessExecution()
        {
            var services = new ServiceCollection();
            services.AddSingleton<CustomerService>();
            services.AddSpider();
            services.AddSpiderRuntimeTracing(tracing => tracing.AddSink<ThrowingTraceSink>());
            using var provider = services.BuildServiceProvider();
            var spider = provider.GetRequiredService<ISpider>();

            var result = await spider
                .InitBridge<CustomerService>()
                .Attach<CustomerCommand, CustomerResult>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), new CustomerCommand("ada@example.com"));

            await provider.GetRequiredService<ISpiderTraceDispatcher>().FlushAsync();

            var trace = await GetOnlyTraceAsync(provider);

            Assert.Equal("ada@example.com", result.Email);
            Assert.Contains(trace.Events, item => item.Kind == SpiderTraceEventKind.PipelineCompleted);
        }

        private static Task ValidateCustomer(CustomerCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                throw new InvalidOperationException("Email is required.");

            return Task.CompletedTask;
        }

        private static CustomerResult MapCustomer(CustomerCommand command)
            => new(command.Email);

        private static async Task<SpiderTrace> GetOnlyTraceAsync(IServiceProvider provider)
        {
            var traces = await GetTracesAsync(provider);
            var trace = Assert.Single(traces);
            return trace;
        }

        private static async Task<IReadOnlyList<SpiderTrace>> GetTracesAsync(IServiceProvider provider)
        {
            var reader = provider.GetRequiredService<ISpiderTraceReader>();
            var traces = new List<SpiderTrace>();
            await foreach (var summary in reader.QueryAsync(new SpiderTraceQuery { Limit = 10 }, CancellationToken.None))
            {
                var storedTrace = await reader.GetAsync(summary.TraceId, CancellationToken.None);
                if (storedTrace != null)
                    traces.Add(storedTrace);
            }

            return traces;
        }

        private sealed record CustomerCommand(string Email);

        private sealed record CustomerResult(string Email);

        private sealed class CustomerService
        {
            public Task<CustomerResult> HandleAsync(CustomerCommand command, CancellationToken cancellationToken)
                => Task.FromResult(new CustomerResult(command.Email));

            public Task<CustomerResult> FailAsync(CustomerCommand command, CancellationToken cancellationToken)
                => throw new InvalidOperationException("Customer failed.");
        }

        private sealed class RecordingBoundary : PipelineExecutionBoundary
        {
        }

        private sealed class RecordingTraceStore : ISpiderTraceStore
        {
            private readonly InMemorySpiderTraceStore _inner = new(new InMemorySpiderTraceStoreOptions());

            public ValueTask AppendAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
                => _inner.AppendAsync(traceEvent, cancellationToken);

            public ValueTask<SpiderTrace> GetAsync(string traceId, CancellationToken cancellationToken)
                => _inner.GetAsync(traceId, cancellationToken);

            public IAsyncEnumerable<SpiderTraceSummary> QueryAsync(SpiderTraceQuery query, CancellationToken cancellationToken)
                => _inner.QueryAsync(query, cancellationToken);
        }

        private sealed class ThrowingTraceSink : ISpiderTraceSink
        {
            public ThrowingTraceSink()
            {
            }

            public ValueTask WriteAsync(SpiderTraceEvent traceEvent, CancellationToken cancellationToken)
                => throw new InvalidOperationException("Sink failed.");
        }
    }
}
