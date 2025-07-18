using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;

namespace Spider.Pipelines.Tests.Extensions
{
    public class ContextExtensionsTests
    {
        [Fact]
        public void IsSuccess_ShouldReturnFalseForPending()
        {
            var context = new ReadOnlyContextStub();
            Assert.False(context.IsSuccess());
        }

        [Fact]
        public void IsFailure_ShouldReturnFalseForPending()
        {
            var context = new ReadOnlyContextStub();
            Assert.False(context.IsFailure());
        }

        [Fact]
        public void IsPending_ShouldReturnTrueForPending()
        {
            var context = new ReadOnlyContextStub();
            Assert.True(context.IsPending());
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
}
