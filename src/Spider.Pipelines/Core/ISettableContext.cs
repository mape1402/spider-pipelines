namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Defines methods for setting request, result state, exception, and pipeline state in a pipeline context.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    public interface ISettableContext<TRequest>
    {
        /// <summary>
        /// Sets the request object for the context.
        /// </summary>
        /// <param name="request">The request object to set.</param>
        void SetRequest(TRequest request);

        /// <summary>
        /// Sets the result state of the context.
        /// </summary>
        /// <param name="state">The result state to set.</param>
        void SetResultState(ResultState state);

        /// <summary>
        /// Sets the exception for the context.
        /// </summary>
        /// <param name="exception">The exception to set.</param>
        void SetException(Exception exception);

        /// <summary>
        /// Sets the pipeline state for the context.
        /// </summary>
        /// <param name="state">The pipeline state to set.</param>
        void SetPipelineState(PipelineState state);
    }

    /// <summary>
    /// Extends <see cref="ISettableContext{TRequest}"/> to include setting a response object in the context.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    public interface ISettableContext<TRequest, TResponse> : ISettableContext<TRequest>
    {
        /// <summary>
        /// Sets the response object for the context.
        /// </summary>
        /// <param name="response">The response object to set.</param>
        void SetResponse(TResponse response);
    }
}
