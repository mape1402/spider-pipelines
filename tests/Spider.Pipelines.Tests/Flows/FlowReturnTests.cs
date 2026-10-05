using Spider.Pipelines.Flows;

namespace Spider.Pipelines.Tests.Flows
{
    public class FlowReturnTests
    {
        [Fact]
        public void Constructor_WhenSyncFactoryIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new FlowReturn<int, int>((Func<int, int>)null));
        }

        [Fact]
        public async Task CreateResponseAsync_WhenSyncFactoryIsConfigured_ShouldReturnResponse()
        {
            var flowReturn = new FlowReturn<int, int>(value => value + 1);

            var response = await flowReturn.CreateResponseAsync(41, CancellationToken.None);

            Assert.Equal(42, response);
        }
    }
}
