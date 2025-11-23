using Business.Interfaces.Implements.Admin;
using Data.Interfaces.Implements.Admin;
using Entity.DTOs.Admin;
using Entity.DTOs.Producer.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardReadRepository _dashboardReadRepository;

        public AdminDashboardService(IAdminDashboardReadRepository dashboardReadRepository)
        {
            _dashboardReadRepository = dashboardReadRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardSnapshotAsync(int top = 5, CancellationToken ct = default)
        {
            var safeLimit = NormalizeLimit(top);

            var orderFunnel = await _dashboardReadRepository.GetOrderFunnelAsync(ct);
            var payment = await _dashboardReadRepository.GetPaymentSummaryAsync(ct);
            var catalog = await _dashboardReadRepository.GetCatalogSummaryAsync(ct);
            var topProducers = await _dashboardReadRepository.GetTopProducersAsync(safeLimit, ct);
            var topProducts = await _dashboardReadRepository.GetTopProductsAsync(safeLimit, ct);

            return new AdminDashboardDto
            {
                OrderFunnel = orderFunnel,
                Payments = payment,
                Catalog = catalog,
                TopProducers = topProducers,
                TopProducts = topProducts
            };
        }

        public Task<OrderFunnelDto> GetOrderFunnelAsync(CancellationToken ct = default)
            => _dashboardReadRepository.GetOrderFunnelAsync(ct);

        public Task<PaymentSummaryDto> GetPaymentSummaryAsync(CancellationToken ct = default)
            => _dashboardReadRepository.GetPaymentSummaryAsync(ct);

        public Task<CatalogSummaryDto> GetCatalogSummaryAsync(CancellationToken ct = default)
            => _dashboardReadRepository.GetCatalogSummaryAsync(ct);

        public Task<IEnumerable<TopProducerStatDto>> GetTopProducersAsync(int limit = 5, CancellationToken ct = default)
            => _dashboardReadRepository.GetTopProducersAsync(NormalizeLimit(limit), ct);

        public Task<IEnumerable<TopProductStatDto>> GetTopProductsAsync(int limit = 5, CancellationToken ct = default)
            => _dashboardReadRepository.GetTopProductsAsync(NormalizeLimit(limit), ct);

        private static int NormalizeLimit(int limit) => limit <= 0 ? 5 : (limit > 20 ? 20 : limit);
    }
}