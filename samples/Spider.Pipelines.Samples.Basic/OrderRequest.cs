namespace Spider.Pipelines.Samples.Basic
{
    /// <summary>
    /// Represents an order placement request.
    /// </summary>
    /// <param name="OrderId">The order identifier.</param>
    /// <param name="Total">The order total.</param>
    public sealed record OrderRequest(string OrderId, decimal Total);
}
