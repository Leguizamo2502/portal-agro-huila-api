using Entity.Domain.Models.Base;
using Entity.Domain.Models.Implements.Auth;
using Entity.Domain.Models.Implements.Producers;

namespace Entity.Domain.Models.Implements.Orders
{
    public class ConsumerRating : BaseModel
    {
        public int OrderId { get; set; }
        public int ProducerId { get; set; }
        public int UserId { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }

        public Order Order { get; set; } = null!;
        public Producer Producer { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
