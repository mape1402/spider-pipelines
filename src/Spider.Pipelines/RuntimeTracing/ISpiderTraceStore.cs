namespace Spider.Pipelines.RuntimeTracing
{
    /// <summary>
    /// Defines a runtime trace store that supports both reads and writes.
    /// </summary>
    public interface ISpiderTraceStore : ISpiderTraceWriter, ISpiderTraceReader
    {
    }
}
