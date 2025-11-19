using Entity.Domain.Models.Base;

namespace Entity.Domain.Models.Implements.Orders.ChatOrder
{
    public class OrderChatConversation : BaseModel
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public bool IsChatEnabled { get; set; }
        public DateTime? ChatEnabledAt { get; set; }
        public DateTime? ChatClosedAt { get; set; }
        public string? ChatClosedReason { get; set; }

        public ICollection<OrderChatMessage> Messages { get; set; } = new List<OrderChatMessage>();
    }
}
