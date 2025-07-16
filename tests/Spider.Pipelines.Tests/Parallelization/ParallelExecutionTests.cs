using Spider.Pipelines.Core;
using Spider.Pipelines.Parallelization;

namespace Spider.Pipelines.Tests.Parallelization
{
    public class ParallelExecutionTests
    {
        [Fact]
        public async Task OnParallelAsync_ShouldInvokeAllDelegates()
        {
            var called = false;
            var delegates = new List<ParallelProcessDelegate<string>>
            {
                (ctx, args) => { called = true; return Task.CompletedTask; }
            };
            var execution = new ParallelExecution<string>(delegates);
            await execution.OnParallelAsync(new ReadOnlyContextStub());
            Assert.True(called);
        }
    }

    public class ParallelExecutionGenericTests
    {
        [Fact]
        public async Task OnParallelAsync_ShouldInvokeAllDelegates()
        {
            var called = false;
            var delegates = new List<ParallelProcessDelegate<string>>
            {
                (ctx, args) => { called = true; return Task.CompletedTask; }
            };
            var execution = new ParallelExecution<string, int>(delegates);
            await execution.OnParallelAsync(new ReadOnlyContextGenericStub());
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
