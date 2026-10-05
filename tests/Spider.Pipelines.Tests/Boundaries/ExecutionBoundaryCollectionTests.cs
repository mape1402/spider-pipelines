using Microsoft.Extensions.DependencyInjection;
using Spider.Pipelines.Boundaries;
using Spider.Pipelines.Boundaries.Internals;

namespace Spider.Pipelines.Tests.Boundaries
{
    public class ExecutionBoundaryCollectionTests
    {
        [Fact]
        public void AddExecutionBoundary_WhenArgumentsAreInvalid_ShouldThrow()
        {
            var collection = new ExecutionBoundaryCollection();

            Assert.Throws<ArgumentNullException>(() => collection.AddExecutionBoundary((Type)null));
            Assert.Throws<InvalidOperationException>(() => collection.AddExecutionBoundary(typeof(string)));
            Assert.Throws<ArgumentNullException>(() => collection.AddExecutionBoundary((Action<IExecutionBoundaryConfiguration>)null));
        }

        [Fact]
        public void CreateExecutionBoundaries_WhenServiceProviderIsNull_ShouldThrow()
        {
            var collection = new ExecutionBoundaryCollection();

            Assert.Throws<ArgumentNullException>(() => collection.CreateExecutionBoundaries(null));
        }

        [Fact]
        public void CreateExecutionBoundaries_ShouldMaterializeConfiguredBoundaries()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TestBoundary>();
            using var provider = services.BuildServiceProvider();
            var collection = new ExecutionBoundaryCollection();

            collection
                .AddExecutionBoundary<TestBoundary>()
                .AddExecutionBoundary(typeof(TestBoundary))
                .AddExecutionBoundary(boundary => boundary.OnBegin((_, _) => ValueTask.CompletedTask));

            var boundaries = collection.CreateExecutionBoundaries(provider);

            Assert.Equal(3, boundaries.Count);
            Assert.Contains(boundaries, boundary => boundary is TestBoundary);
            Assert.Contains(boundaries, boundary => boundary.GetType() == typeof(DelegateExecutionBoundary));
        }

        private sealed class TestBoundary : PipelineExecutionBoundary
        {
        }
    }
}
