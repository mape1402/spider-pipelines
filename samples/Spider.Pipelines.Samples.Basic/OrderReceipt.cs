namespace Spider.Pipelines.Samples.Basic
{
    /// <summary>
    /// Represents a receipt returned by the order service.
    /// </summary>
    /// <param name="ReceiptId">The generated receipt identifier.</param>
    /// <param name="Total">The receipted total.</param>
    public sealed record OrderReceipt(string ReceiptId, decimal Total);
}
