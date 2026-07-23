using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;

namespace Spider.Pipelines.Tests.Boundaries
{
    public class PipelineExecutionBoundaryTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenNoBoundariesAreRegistered_ShouldKeepCurrentBehavior()
        {
            var bridge = CreateBridge();

            var result = await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(6, result);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldKeepBoundaryActiveThroughPreTargetAndPost()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder =>
                {
                    builder
                        .PreProcess<string, int>((ctx, args) =>
                        {
                            log.Add(log.Active ? "pre-active" : "pre-inactive");
                            return Task.CompletedTask;
                        })
                        .OnSuccess<string, int>((ctx, args) =>
                        {
                            log.Add(log.Active ? "post-active" : "post-inactive");
                            return Task.CompletedTask;
                        });
                })
                .ExecuteAsync(service => (request, token) => service.HandleWithBoundaryCheckAsync(request, token), "spider");

            Assert.Contains("pre-active", log.Events);
            Assert.Contains("handler-active", log.Events);
            Assert.Contains("post-active", log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPipelineSucceeds_ShouldCompleteAndDisposeBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:complete", "boundary:dispose" }, log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPreProcessThrows_ShouldFaultAndDisposeBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder =>
                {
                    builder.PreProcess<string, int>((ctx, args) => throw new InvalidOperationException("Pre failed."));
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException", "boundary:dispose" }, log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenHandlerThrows_ShouldFaultAndDisposeBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.ThrowAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException", "boundary:dispose" }, log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPostProcessThrows_ShouldFaultAndDisposeBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder =>
                {
                    builder.OnSuccess<string, int>((ctx, args) => throw new InvalidOperationException("Post failed."));
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "boundary:begin", "boundary:fault:InvalidOperationException", "boundary:dispose" }, log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPipelineCancels_ShouldCancelAndDisposeBoundary()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider().AddExecutionBoundary<RecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder =>
                {
                    builder.PreProcess<string, int>((ctx, args) =>
                    {
                        ctx.CancelOperation();
                        return Task.CompletedTask;
                    });
                })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "boundary:begin", "boundary:cancel", "boundary:dispose" }, log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenMultipleBoundariesAreRegistered_ShouldNestAndDisposeInReverseOrder()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider()
                    .AddExecutionBoundary<FirstRecordingBoundary>()
                    .AddExecutionBoundary<SecondRecordingBoundary>();
            });

            await bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider");

            Assert.Equal(new[] { "first:begin", "second:begin", "second:complete", "first:complete", "second:dispose", "first:dispose" }, log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenSecondBoundaryBeginFails_ShouldFaultAndDisposeOpenedScopes()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider()
                    .AddExecutionBoundary<FirstRecordingBoundary>()
                    .AddExecutionBoundary<ThrowingBeginBoundary>();
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal(new[] { "first:begin", "throw-begin:begin", "first:fault:InvalidOperationException", "first:dispose" }, log.Events);
        }

        [Fact]
        public async Task ExecuteAsync_WhenCompleteFails_ShouldDisposeOpenedScopesAndSurfaceCompleteException()
        {
            var log = new BoundaryEventLog();
            var bridge = CreateBridge(services =>
            {
                services.AddSingleton(log);
                services.AddSpider()
                    .AddExecutionBoundary<FirstRecordingBoundary>()
                    .AddExecutionBoundary<ThrowingCompleteBoundary>();
            });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => bridge
                .Attach<string, int>(builder => { })
                .ExecuteAsync(service => (request, token) => service.HandleAsync(request, token), "spider"));

            Assert.Equal("Complete failed.", exception.Message);
            Assert.Equal(new[] { "first:begin", "throw-complete:begin", "throw-complete:complete", "throw-complete:dispose", "first:dispose" }, log.Events);
        }

        /// <summary>
        /// Creates a service bridge for boundary tests.
        /// </summary>
        /// <param name="configure">An optional service configuration action.</param>
        /// <returns>A service bridge for the test service.</returns>
        private static IServiceBridge<BoundaryTestService> CreateBridge(Action<IServiceCollection> configure = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton<BoundaryTestService>();

            if (configure == null)
                services.AddSpider();
            else
                configure(services);

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<ISpider>().InitBridge<BoundaryTestService>();
        }
    }

    /// <summary>
    /// Records boundary test events.
    /// </summary>
    public sealed class BoundaryEventLog
    {
        private readonly IList<string> _events = new List<string>();

        /// <summary>
        /// Gets a value indicating whether a test boundary is active.
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Gets recorded events.
        /// </summary>
        public IReadOnlyCollection<string> Events => _events.ToArray();

        /// <summary>
        /// Records an event.
        /// </summary>
        /// <param name="eventName">The event name to record.</param>
        public void Add(string eventName)
            => _events.Add(eventName);

        /// <summary>
        /// Marks the boundary as active.
        /// </summary>
        public void Activate()
            => Active = true;

        /// <summary>
        /// Marks the boundary as inactive.
        /// </summary>
        public void Deactivate()
            => Active = false;
    }

    /// <summary>
    /// Provides service methods used by boundary tests.
    /// </summary>
    public sealed class BoundaryTestService
    {
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundaryTestService"/> class.
        /// </summary>
        /// <param name="log">The optional boundary event log.</param>
        public BoundaryTestService(BoundaryEventLog log = null)
        {
            _log = log;
        }

        /// <summary>
        /// Handles a request by returning its length.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The request length.</returns>
        public Task<int> HandleAsync(string request, CancellationToken cancellationToken)
            => Task.FromResult(request.Length);

        /// <summary>
        /// Handles a request and records whether the boundary is active.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The request length.</returns>
        public Task<int> HandleWithBoundaryCheckAsync(string request, CancellationToken cancellationToken)
        {
            _log.Add(_log.Active ? "handler-active" : "handler-inactive");
            return Task.FromResult(request.Length);
        }

        /// <summary>
        /// Throws an exception for boundary fault tests.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that never completes successfully.</returns>
        public Task<int> ThrowAsync(string request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Handler failed.");
    }

    /// <summary>
    /// Records a generic boundary named "boundary".
    /// </summary>
    public sealed class RecordingBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public RecordingBoundary(BoundaryEventLog log) : base("boundary", log) { }
    }

    /// <summary>
    /// Records a boundary named "first".
    /// </summary>
    public sealed class FirstRecordingBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstRecordingBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public FirstRecordingBoundary(BoundaryEventLog log) : base("first", log) { }
    }

    /// <summary>
    /// Records a boundary named "second".
    /// </summary>
    public sealed class SecondRecordingBoundary : NamedBoundary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondRecordingBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public SecondRecordingBoundary(BoundaryEventLog log) : base("second", log) { }
    }

    /// <summary>
    /// Provides a named test boundary.
    /// </summary>
    public abstract class NamedBoundary : IPipelineExecutionBoundary
    {
        private readonly string _name;
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedBoundary"/> class.
        /// </summary>
        /// <param name="name">The boundary name.</param>
        /// <param name="log">The event log.</param>
        protected NamedBoundary(string name, BoundaryEventLog log)
        {
            _name = name;
            _log = log;
        }

        /// <inheritdoc/>
        public virtual ValueTask<IPipelineExecutionBoundaryScope> BeginAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:begin");
            _log.Activate();
            return ValueTask.FromResult<IPipelineExecutionBoundaryScope>(new NamedBoundaryScope(_name, _log));
        }
    }

    /// <summary>
    /// Provides a boundary that throws while beginning.
    /// </summary>
    public sealed class ThrowingBeginBoundary : IPipelineExecutionBoundary
    {
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingBeginBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public ThrowingBeginBoundary(BoundaryEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask<IPipelineExecutionBoundaryScope> BeginAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            _log.Add("throw-begin:begin");
            throw new InvalidOperationException("Begin failed.");
        }
    }

    /// <summary>
    /// Provides a boundary that throws while completing.
    /// </summary>
    public sealed class ThrowingCompleteBoundary : IPipelineExecutionBoundary
    {
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingCompleteBoundary"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public ThrowingCompleteBoundary(BoundaryEventLog log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public ValueTask<IPipelineExecutionBoundaryScope> BeginAsync(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            _log.Add("throw-complete:begin");
            _log.Activate();
            return ValueTask.FromResult<IPipelineExecutionBoundaryScope>(new ThrowingCompleteBoundaryScope(_log));
        }
    }

    /// <summary>
    /// Provides a named boundary test scope.
    /// </summary>
    public class NamedBoundaryScope : IPipelineExecutionBoundaryScope
    {
        private readonly string _name;
        private readonly BoundaryEventLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedBoundaryScope"/> class.
        /// </summary>
        /// <param name="name">The scope name.</param>
        /// <param name="log">The event log.</param>
        public NamedBoundaryScope(string name, BoundaryEventLog log)
        {
            _name = name;
            _log = log;
        }

        /// <inheritdoc/>
        public virtual ValueTask CompleteAsync(CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:complete");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual ValueTask FaultAsync(Exception exception, CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:fault:{exception.GetType().Name}");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual ValueTask CancelAsync(CancellationToken cancellationToken)
        {
            _log.Add($"{_name}:cancel");
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual ValueTask DisposeAsync()
        {
            _log.Add($"{_name}:dispose");
            _log.Deactivate();
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Provides a boundary scope that throws while completing.
    /// </summary>
    public sealed class ThrowingCompleteBoundaryScope : NamedBoundaryScope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowingCompleteBoundaryScope"/> class.
        /// </summary>
        /// <param name="log">The event log.</param>
        public ThrowingCompleteBoundaryScope(BoundaryEventLog log) : base("throw-complete", log) { }

        /// <inheritdoc/>
        public override ValueTask CompleteAsync(CancellationToken cancellationToken)
        {
            base.CompleteAsync(cancellationToken);
            throw new InvalidOperationException("Complete failed.");
        }
    }
}
