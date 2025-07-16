using Spider.Pipelines.Core;
using Spider.Pipelines.PostProcessing;

namespace Spider.Pipelines.Tests.PostProcessing
{
    public class PostProcessExecutionTests
    {
        [Fact]
        public async Task OnSuccessAsync_ShouldInvokeAllDelegates()
        {
            var called = false;
            var delegates = new List<SuccessPostProcessDelegate<string>>
            {
                (ctx, args) => { called = true; return Task.CompletedTask; }
            };
            var execution = new PostProcessExecution<string>(delegates, new List<FailurePostProcessDelegate<string>>());
            await execution.OnSuccessAsync(new ReadOnlyContextStub());
            Assert.True(called);
        }

        [Fact]
        public async Task OnFailureAsync_ShouldInvokeAllDelegates()
        {
            var called = false;
            var delegates = new List<FailurePostProcessDelegate<string>>
            {
                (ctx, args) => { called = true; return Task.CompletedTask; }
            };
            var execution = new PostProcessExecution<string>(new List<SuccessPostProcessDelegate<string>>(), delegates);
            await execution.OnFailureAsync(new ReadOnlyContextStub());
            Assert.True(called);
        }
    }

    public class PostProcessExecutionGenericTests
    {
        [Fact]
        public async Task OnSuccessAsync_ShouldInvokeAllDelegates()
        {
            var called = false;
            var delegates = new List<SuccessPostProcessDelegate<string, int>>
            {
                (ctx, args) => { called = true; return Task.CompletedTask; }
            };
            var execution = new PostProcessExecution<string, int>(delegates, new List<FailurePostProcessDelegate<string>>());
            await execution.OnSuccessAsync(new ReadOnlyContextGenericStub());
            Assert.True(called);
        }

        [Fact]
        public async Task OnFailureAsync_ShouldInvokeAllDelegates()
        {
            var called = false;
            var delegates = new List<FailurePostProcessDelegate<string>>
            {
                (ctx, args) => { called = true; return Task.CompletedTask; }
            };
            var execution = new PostProcessExecution<string, int>(new List<SuccessPostProcessDelegate<string, int>>(), delegates);
            await execution.OnFailureAsync(new ReadOnlyContextStub());
            Assert.True(called);
        }
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

    // Stub for IReadOnlyContext<string, int>
    public class ReadOnlyContextGenericStub : IReadOnlyContext<string, int>
    {
        public string Request => "test";
        public int Response => 42;
        public IServiceProvider Services => null;
        public bool Cancelled => false;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }
}
