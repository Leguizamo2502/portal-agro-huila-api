using Entity.Domain.Models.Implements.Orders.ChatOrder;

namespace Data.Interfaces.Implements.Orders.OrderChat
{
    public interface IOrderChatMessageRepository
    {
        Task<OrderChatMessage> AddAsync(OrderChatMessage message);
        Task<IReadOnlyList<OrderChatMessage>> GetMessagesAsync(int conversationId, int skip, int take);
        Task<int> CountAsync(int conversationId);
    }
}
