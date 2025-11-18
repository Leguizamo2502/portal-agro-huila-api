using Entity.DTOs.Admin;
using Entity.DTOs.Producer.Analytics;

namespace Business.Interfaces.Implements.Admin
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardSnapshotAsync(int top = 5, CancellationToken ct = default);
        Task<OrderFunnelDto> GetOrderFunnelAsync(CancellationToken ct = default);
        Task<PaymentSummaryDto> GetPaymentSummaryAsync(CancellationToken ct = default);
        Task<CatalogSummaryDto> GetCatalogSummaryAsync(CancellationToken ct = default);
        Task<IEnumerable<TopProducerStatDto>> GetTopProducersAsync(int limit = 5, CancellationToken ct = default);
        Task<IEnumerable<TopProductStatDto>> GetTopProductsAsync(int limit = 5, CancellationToken ct = default);
    }
}
