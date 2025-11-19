using Entity.DTOs.Orders.OrderChat;

namespace Web.Hubs.Interfaces.OrderChat
{
    public interface IOrderChatClient
    {
        Task ReceiveMessage(string orderCode, OrderChatMessageDto message);
    }
}
