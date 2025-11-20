using Business.Interfaces.Implements.Orders.OrderChat;
using Entity.DTOs.Orders.OrderChat;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs.Interfaces.OrderChat;

namespace Web.Hubs.Implements.OrderChat
{
    public class SignalROrderChatMessagePusher : IOrderChatMessagePusher
    {
        private readonly IHubContext<OrderChatHub, IOrderChatClient> _hubContext;

        public SignalROrderChatMessagePusher(IHubContext<OrderChatHub, IOrderChatClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task BroadcastMessageAsync(
            string orderCode,
            OrderChatMessageDto customerMessage,
            OrderChatMessageDto producerMessage,
            int customerUserId,
            int producerUserId,
            CancellationToken ct = default)
        {
            if (customerUserId == producerUserId)
            {
                return _hubContext.Clients
                    .User(customerUserId.ToString())
                    .ReceiveMessage(orderCode, customerMessage);
            }

            var customerTask = _hubContext.Clients
                .User(customerUserId.ToString())
                .ReceiveMessage(orderCode, customerMessage);

            var producerTask = _hubContext.Clients
                .User(producerUserId.ToString())
                .ReceiveMessage(orderCode, producerMessage);

            return Task.WhenAll(customerTask, producerTask);
        }
    }
}