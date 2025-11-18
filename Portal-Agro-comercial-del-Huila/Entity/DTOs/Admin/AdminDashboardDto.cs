using Entity.DTOs.Producer.Analytics;

namespace Entity.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public OrderFunnelDto OrderFunnel { get; set; } = new();
        public PaymentSummaryDto Payments { get; set; } = new();
        public CatalogSummaryDto Catalog { get; set; } = new();
        public IEnumerable<TopProducerStatDto> TopProducers { get; set; } = Array.Empty<TopProducerStatDto>();
        public IEnumerable<TopProductStatDto> TopProducts { get; set; } = Array.Empty<TopProductStatDto>();
    }
}
