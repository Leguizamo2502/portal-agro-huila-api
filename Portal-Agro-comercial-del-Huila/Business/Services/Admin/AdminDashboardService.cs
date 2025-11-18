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

            var orderFunnelTask = _dashboardReadRepository.GetOrderFunnelAsync(ct);
            var paymentTask = _dashboardReadRepository.GetPaymentSummaryAsync(ct);
            var catalogTask = _dashboardReadRepository.GetCatalogSummaryAsync(ct);
            var topProducersTask = _dashboardReadRepository.GetTopProducersAsync(safeLimit, ct);
            var topProductsTask = _dashboardReadRepository.GetTopProductsAsync(safeLimit, ct);

            await Task.WhenAll(orderFunnelTask, paymentTask, catalogTask, topProducersTask, topProductsTask);

            return new AdminDashboardDto
            {
                OrderFunnel = orderFunnelTask.Result,
                Payments = paymentTask.Result,
                Catalog = catalogTask.Result,
                TopProducers = topProducersTask.Result,
                TopProducts = topProductsTask.Result
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