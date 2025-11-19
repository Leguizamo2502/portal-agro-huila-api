using Business.Interfaces.Implements.Orders.OrderChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Utilities.Exceptions;
using Web.Hubs.Interfaces.OrderChat;

namespace Web.Hubs.Implements.OrderChat
{
    [Authorize]
    public class OrderChatHub : Hub<IOrderChatClient>
    {
        private readonly IOrderChatService _orderChatService;

        public OrderChatHub(IOrderChatService orderChatService)
        {
            _orderChatService = orderChatService;
        }

        public async Task JoinOrderRoom(string orderCode)
        {
            var normalizedCode = NormalizeOrderCode(orderCode);
            var userId = GetUserId();

            try
            {
                await _orderChatService.EnsureParticipantAsync(userId, normalizedCode);
            }
            catch (BusinessException ex)
            {
                throw new HubException(ex.Message);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, BuildGroupName(normalizedCode));
        }

        public async Task LeaveOrderRoom(string orderCode)
        {
            var normalizedCode = NormalizeOrderCode(orderCode);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildGroupName(normalizedCode));
        }

        private int GetUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)
                        ?? Context.User?.FindFirst("sub");

            if (claim == null || !int.TryParse(claim.Value, out var userId))
            {
                throw new HubException("No se pudo identificar el usuario para el chat.");
            }

            return userId;
        }

        private static string NormalizeOrderCode(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                throw new HubException("El código del pedido es obligatorio.");
            }

            return orderCode.Trim();
        }

        private static string BuildGroupName(string orderCode) => $"order-chat-{orderCode}";
    }
}
