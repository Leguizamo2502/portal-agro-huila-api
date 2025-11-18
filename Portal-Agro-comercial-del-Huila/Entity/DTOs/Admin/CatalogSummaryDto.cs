namespace Entity.DTOs.Admin
{
    public class CatalogSummaryDto
    {
        public int ActiveProducers { get; set; }
        public int TotalProducts { get; set; }
        public int PublishedProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int Categories { get; set; }
        public int Favorites { get; set; }
    }
}
