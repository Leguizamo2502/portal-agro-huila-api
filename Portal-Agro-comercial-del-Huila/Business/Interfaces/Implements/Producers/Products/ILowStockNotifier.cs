namespace Business.Interfaces.Implements.Producers.Products
{
    public interface ILowStockNotifier
    {
        Task NotifyIfLowAsync(int productId, int currentStock, CancellationToken ct = default);
    }
}
