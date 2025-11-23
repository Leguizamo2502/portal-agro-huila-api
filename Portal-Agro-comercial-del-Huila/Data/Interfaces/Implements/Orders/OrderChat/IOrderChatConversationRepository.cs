using Data.Interfaces.IRepository;
using Entity.Domain.Models.Implements.Orders.ChatOrder;

namespace Data.Interfaces.Implements.Orders.OrderChat
{
    public interface IOrderChatConversationRepository : IDataGeneric<OrderChatConversation>
    {
        Task<OrderChatConversation?> GetByOrderIdAsync(int orderId);
        Task<OrderChatConversation?> GetByOrderIdTrackingAsync(int orderId);
    }
}
