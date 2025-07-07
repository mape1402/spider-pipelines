namespace Spider.Pipelines.Core
{
    /// <summary>
    /// Defines a contract for a context that supports cancellation of an operation.
    /// </summary>
    public interface ICancellableContext
    {
        /// <summary>
        /// Cancels the ongoing operation associated with the context.
        /// </summary>
        void CancelOperation();
    }
}
