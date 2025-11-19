using Entity.Domain.Models.Base;

namespace Entity.Domain.Models.Implements.Orders.ChatOrder
{
    public class OrderChatMessage : BaseModel
    {
        public int ConversationId { get; set; }
        public OrderChatConversation Conversation { get; set; } = null!;

        public int SenderUserId { get; set; }
        public string Message { get; set; } = null!;
        public bool IsSystem { get; set; }
    }
}
