using Entity.DTOs.Admin;
using Entity.DTOs.Producer.Analytics;

namespace Data.Interfaces.Implements.Admin
{
    public interface IAdminDashboardReadRepository
    {
        Task<OrderFunnelDto> GetOrderFunnelAsync(CancellationToken ct = default);
        Task<PaymentSummaryDto> GetPaymentSummaryAsync(CancellationToken ct = default);
        Task<CatalogSummaryDto> GetCatalogSummaryAsync(CancellationToken ct = default);
        Task<IEnumerable<TopProducerStatDto>> GetTopProducersAsync(int limit, CancellationToken ct = default);
        Task<IEnumerable<TopProductStatDto>> GetTopProductsAsync(int limit, CancellationToken ct = default);
    }
}
