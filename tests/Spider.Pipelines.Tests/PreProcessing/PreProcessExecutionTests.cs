using Spider.Pipelines.Core;
using Spider.Pipelines.PreProcessing;

namespace Spider.Pipelines.Tests.PreProcessing
{
    public class PreProcessExecutionTests
    {
        [Fact]
        public async Task OnPreProcessAsync_ShouldInvokeAllDelegates()
        {
            var called = false;
            var delegates = new List<PreProcessDelegate<string>>
            {
                (ctx, args) => { called = true; return Task.CompletedTask; }
            };
            var execution = new PreProcessExecution<string>(delegates);
            await execution.OnPreProcessAsync(new ReadOnlyContextStub());
            Assert.True(called);
        }

        [Fact]
        public async Task OnPreProcessAsync_ShouldCancelIfDelegateCancels()
        {
            var delegates = new List<PreProcessDelegate<string>>
            {
                (ctx, args) => { args.Cancelled = true; return Task.CompletedTask; },
                (ctx, args) => throw new Exception("Should not be called")
            };
            var context = new CancellableContextStub();
            var execution = new PreProcessExecution<string>(delegates);
            await execution.OnPreProcessAsync(context);
            Assert.True(context.CancelledCalled);
        }

        // Stub for IReadOnlyContext<string>
        public class ReadOnlyContextStub : IReadOnlyContext<string>
        {
            public string Request => "test";
            public IServiceProvider Services => null;
            public bool Cancelled => false;
            public PipelineState PipelineState => PipelineState.OnPreProcess;
            public CancellationToken CancellationToken => CancellationToken.None;
            public ResultState ResultState => ResultState.Pending;
            public Exception Exception => null;
        }

        // Stub for ICancellableContext
        public class CancellableContextStub : IReadOnlyContext<string>, ICancellableContext
        {
            public bool CancelledCalled { get; private set; }
            public string Request => "test";
            public IServiceProvider Services => null;
            public bool Cancelled => false;
            public PipelineState PipelineState => PipelineState.OnPreProcess;
            public CancellationToken CancellationToken => CancellationToken.None;
            public ResultState ResultState => ResultState.Pending;
            public Exception Exception => null;
            public void CancelOperation() => CancelledCalled = true;
        }
    }
}
