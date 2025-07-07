namespace Spider.Pipelines.PreProcessing
{
    using Spider.Pipelines.Core;
    using Spider.Pipelines.Extensions;

    /// <summary>
    /// Executes a collection of preprocessing delegates before the main pipeline operation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    internal sealed class PreProcessExecution<TRequest> : IPreProcessExecution<TRequest>
    {
        /// <summary>
        /// The collection of delegates to execute during preprocessing.
        /// </summary>
        private readonly IReadOnlyCollection<PreProcessDelegate<TRequest>> _preProcessDelegates;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreProcessExecution{TRequest}"/> class.
        /// </summary>
        /// <param name="prepareDelegates">The collection of preprocessing delegates to execute.</param>
        public PreProcessExecution(IReadOnlyCollection<PreProcessDelegate<TRequest>> prepareDelegates)
        {
            _preProcessDelegates = prepareDelegates ?? Array.Empty<PreProcessDelegate<TRequest>>();
        }

        /// <inheritdoc />
        public async Task OnPreProcessAsync(IReadOnlyContext<TRequest> context)
        {
            if (!_preProcessDelegates.Any())
                return;

            //TODO: Get standard configuration
            var arguments = new PreProcessArguments();

            foreach (var preProcess in _preProcessDelegates)
            {
                await preProcess(context, arguments);

                if (arguments.Cancelled)
                {
                    context.CancelOperation();
                    break;
                }
            }
        }
    }
}