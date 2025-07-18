using Spider.Pipelines.Core;

namespace Spider.Pipelines.Tests.Core
{
    public class ContextBaseTests
    {
        [Fact]
        public void CancelOperation_ShouldSetCancelledTrue()
        {
            var context = new TestContext(new ServiceProviderStub(), CancellationToken.None);
            Assert.False(context.Cancelled);
            context.CancelOperation();
            Assert.True(context.Cancelled);
        }
    }

    public class TestContext : Context
    {
        public TestContext(IServiceProvider services, CancellationToken cancellationToken) : base(services, cancellationToken) { }
    }
}
