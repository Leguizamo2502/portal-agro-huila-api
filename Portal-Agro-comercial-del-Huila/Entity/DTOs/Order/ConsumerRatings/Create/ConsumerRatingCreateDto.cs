using Entity.DTOs.BaseDTO;

namespace Entity.DTOs.Order.ConsumerRatings.Create
{
    public class ConsumerRatingCreateDto : BaseDto
    {
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public string RowVersion { get; set; } = null!;
    }
}
