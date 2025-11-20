using Entity.DTOs.Orders.OrderChat;

namespace Business.Interfaces.Implements.Orders.OrderChat
{
    public interface IOrderChatService
    {
        Task EnsureConversationForOrderAsync(int orderId);
        Task AddSystemMessageAsync(int orderId, string message);
        Task EnableConversationAsync(int orderId);
        Task CloseConversationAsync(int orderId, string? systemMessage = null);
        Task EnsureParticipantAsync(int userId, string orderCode);
        Task<OrderChatMessageDto> SendMessageAsync(int userId, string orderCode, OrderChatMessageCreateDto dto);
        Task<OrderChatMessagesPageDto> GetMessagesAsync(int userId, string orderCode, int skip, int take);
    }
}
