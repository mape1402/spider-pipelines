using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.RuntimeTracing;
using Spider.Pipelines.RuntimeTracing.Internals;

namespace Spider.Pipelines.Tests.RuntimeTracing
{
    public class SpiderRuntimeTracingBuilderTests
    {
        [Fact]
        public void Constructor_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var services = new ServiceCollection();
            var options = new SpiderRuntimeTracingOptions();

            Assert.Throws<ArgumentNullException>(() => new SpiderRuntimeTracingBuilder(null, options));
            Assert.Throws<ArgumentNullException>(() => new SpiderRuntimeTracingBuilder(services, null));
        }

        [Fact]
        public void Properties_ShouldUpdateOptions()
        {
            var services = new ServiceCollection();
            var options = new SpiderRuntimeTracingOptions();
            var builder = new SpiderRuntimeTracingBuilder(services, options)
            {
                Verbosity = SpiderTraceVerbosity.Off,
                QueueCapacity = 42,
                Backpressure = SpiderTraceBackpressure.Block
            };

            Assert.Equal(SpiderTraceVerbosity.Off, builder.Verbosity);
            Assert.Equal(42, builder.QueueCapacity);
            Assert.Equal(SpiderTraceBackpressure.Block, builder.Backpressure);
            Assert.Same(options, builder.Options);
            Assert.Same(services, builder.Services);
        }

        [Fact]
        public async Task NullTracer_ShouldIgnoreSpansAndEvents()
        {
            var tracer = NullSpiderRuntimeTracer.Instance;

            var scope = await tracer.StartSpanAsync(new SpiderTraceSpanDefinition());
            await tracer.AddEventAsync(new SpiderTraceEvent());

            Assert.False(tracer.IsEnabled);
            Assert.Null(scope);
        }

        [Fact]
        public void SpiderTraceException_ShouldSummarizeExceptions()
        {
            var exception = new InvalidOperationException("Runtime failed.");

            var summary = SpiderTraceException.FromException(exception);
            var manual = new SpiderTraceException("Type", "Message", "Stack");

            Assert.Null(SpiderTraceException.FromException(null));
            Assert.Equal(typeof(InvalidOperationException).FullName, summary.Type);
            Assert.Equal("Runtime failed.", summary.Message);
            Assert.Equal("Type", manual.Type);
            Assert.Equal("Message", manual.Message);
            Assert.Equal("Stack", manual.StackTrace);
        }
    }
}
