namespace Entity.DTOs.Admin
{
    public class OrderFunnelDto
    {
        public int TotalOrders { get; set; }
        public IReadOnlyList<OrderStatusBucketDto> Buckets { get; set; } = Array.Empty<OrderStatusBucketDto>();
    }
}
