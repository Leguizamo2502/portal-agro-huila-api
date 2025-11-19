using Entity.DTOs.Orders.OrderChat;

namespace Business.Interfaces.Implements.Orders.OrderChat
{
    public interface IOrderChatMessagePusher
    {
        Task BroadcastMessageAsync(
            string orderCode,
            OrderChatMessageDto customerMessage,
            OrderChatMessageDto producerMessage,
            int customerUserId,
            int producerUserId,
            CancellationToken ct = default);
    }
}
