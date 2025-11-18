namespace Entity.DTOs.Admin
{
    public class TopProducerStatDto
    {
        public int ProducerId { get; set; }
        public string ProducerName { get; set; } = string.Empty;
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
