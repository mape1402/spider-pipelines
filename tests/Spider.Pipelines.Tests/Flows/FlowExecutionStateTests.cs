using Spider.Pipelines.Flows.Internals;

namespace Spider.Pipelines.Tests.Flows
{
    public class FlowExecutionStateTests
    {
        [Fact]
        public void Constructor_WhenInitialTypeIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new FlowExecutionState(null, new BaseHistoryValue(), null));
        }

        [Fact]
        public void Resolve_WhenExactValueExists_ShouldReturnIt()
        {
            var value = new BaseHistoryValue();
            var state = new FlowExecutionState(typeof(BaseHistoryValue), value, null);

            var resolved = state.Resolve<BaseHistoryValue>();

            Assert.Same(value, resolved);
        }

        [Fact]
        public void Resolve_WhenOneAssignableValueExists_ShouldReturnIt()
        {
            var value = new DerivedHistoryValue();
            var state = new FlowExecutionState(typeof(DerivedHistoryValue), value, null);

            var resolved = state.Resolve<BaseHistoryValue>();

            Assert.Same(value, resolved);
        }

        [Fact]
        public void Resolve_WhenMultipleExactValuesExist_ShouldThrow()
        {
            var state = new FlowExecutionState(typeof(BaseHistoryValue), new BaseHistoryValue(), null);
            state.SetActive(typeof(BaseHistoryValue), new BaseHistoryValue());

            Assert.Throws<InvalidOperationException>(() => state.Resolve<BaseHistoryValue>());
        }

        [Fact]
        public void Resolve_WhenMultipleAssignableValuesExist_ShouldThrow()
        {
            var state = new FlowExecutionState(typeof(DerivedHistoryValue), new DerivedHistoryValue(), null);
            state.SetActive(typeof(SecondDerivedHistoryValue), new SecondDerivedHistoryValue());

            Assert.Throws<InvalidOperationException>(() => state.Resolve<BaseHistoryValue>());
        }

        [Fact]
        public void Resolve_WhenValueDoesNotExist_ShouldThrow()
        {
            var state = new FlowExecutionState(typeof(BaseHistoryValue), new BaseHistoryValue(), null);

            Assert.Throws<InvalidOperationException>(() => state.Resolve<MissingHistoryValue>());
        }

        [Fact]
        public void SetActive_WhenTypeIsNull_ShouldThrow()
        {
            var state = new FlowExecutionState(typeof(BaseHistoryValue), new BaseHistoryValue(), null);

            Assert.Throws<ArgumentNullException>(() => state.SetActive(null, new BaseHistoryValue()));
        }

        [Fact]
        public void Stop_ShouldMarkStateAsStopped()
        {
            var state = new FlowExecutionState(typeof(BaseHistoryValue), new BaseHistoryValue(), null);

            state.Stop();

            Assert.True(state.IsStopped);
        }

        private class BaseHistoryValue { }

        private sealed class DerivedHistoryValue : BaseHistoryValue { }

        private sealed class SecondDerivedHistoryValue : BaseHistoryValue { }

        private sealed class MissingHistoryValue { }
    }
}
