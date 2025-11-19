using Entity.DTOs.BaseDTO;

namespace Entity.DTOs.Order.ConsumerRatings.Select
{
    public class ConsumerRatingSelectDto : BaseDto
    {
        public int OrderId { get; set; }
        public int ProducerId { get; set; }
        public int UserId { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreateAt { get; set; }
        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
    }
}
