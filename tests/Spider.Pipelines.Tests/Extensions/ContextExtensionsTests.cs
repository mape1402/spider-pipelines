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

        [Fact]
        public void IsCancelledResult_ShouldReturnTrueForCancelled()
        {
            var context = new CancelledContextStub();
            Assert.True(context.IsCancelledResult());
        }

        [Fact]
        public void BaseCancelOperation_ShouldCancelConcreteContext()
        {
            IReadOnlyContext context = new Context<string>("request", null);

            context.CancelOperation();

            Assert.True(context.Cancelled);
            Assert.Equal(ResultState.Cancelled, context.ResultState);
        }

        [Fact]
        public void GenericContextExtensions_ShouldSetPipelineAndResultState()
        {
            IReadOnlyContext<string> readOnly = new Context<string>("request", null);
            var settable = readOnly.AsSettable();

            readOnly.OnTargeting();
            Assert.Equal(PipelineState.OnTargeting, readOnly.PipelineState);

            readOnly.OnPostProcess();
            Assert.Equal(PipelineState.OnPostProcess, readOnly.PipelineState);

            settable.Failure(new InvalidOperationException("failed"));
            Assert.True(readOnly.IsFailure());
            Assert.Equal("failed", readOnly.Exception.Message);

            settable.Cancelled();
            Assert.True(readOnly.IsCancelledResult());

            readOnly.CancelOperation();
            Assert.True(readOnly.IsCancelled());
        }

        [Fact]
        public void GenericResponseContextExtensions_ShouldSetPipelineResultAndResponse()
        {
            IReadOnlyContext<string, int> readOnly = new Context<string, int>("request", null);
            var settable = readOnly.AsSettable();

            readOnly.OnPreProcess();
            Assert.Equal(PipelineState.OnPreProcess, readOnly.PipelineState);

            readOnly.OnTargeting();
            Assert.Equal(PipelineState.OnTargeting, readOnly.PipelineState);

            readOnly.OnPostProcess();
            Assert.Equal(PipelineState.OnPostProcess, readOnly.PipelineState);

            settable.Success(42);
            Assert.True(readOnly.IsSuccess());
            Assert.Equal(42, readOnly.Response);

            settable.Failure();
            Assert.True(readOnly.IsFailure());

            settable.Cancelled();
            Assert.True(readOnly.IsCancelledResult());

            readOnly.CancelOperation();
            Assert.True(readOnly.IsCancelled());
        }

        [Fact]
        public void ContextExtensions_WhenContextDoesNotSupportMutation_ShouldThrow()
        {
            var requestOnly = new ReadOnlyContextStub();
            var responseContext = new ReadOnlyResponseContextStub();

            Assert.Throws<InvalidCastException>(() => requestOnly.AsSettable());
            Assert.Throws<InvalidCastException>(() => responseContext.AsSettable());
            Assert.Throws<InvalidCastException>(() => ((IReadOnlyContext)requestOnly).AsCancellable());
            Assert.Throws<InvalidCastException>(() => requestOnly.AsCancellable());
            Assert.Throws<InvalidCastException>(() => responseContext.AsCancellable());
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

    public class CancelledContextStub : IReadOnlyContext<string>
    {
        public string Request => "test";
        public IServiceProvider Services => null;
        public bool Cancelled => true;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Cancelled;
        public Exception Exception => null;
    }

    public class ReadOnlyResponseContextStub : IReadOnlyContext<string, int>
    {
        public string Request => "test";
        public int Response => 0;
        public IServiceProvider Services => null;
        public bool Cancelled => false;
        public PipelineState PipelineState => PipelineState.OnPreProcess;
        public CancellationToken CancellationToken => CancellationToken.None;
        public ResultState ResultState => ResultState.Pending;
        public Exception Exception => null;
    }
}
