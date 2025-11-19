using Data.Interfaces.Implements.Orders.OrderChat;
using Data.Repository;
using Entity.Domain.Models.Implements.Orders.ChatOrder;
using Entity.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Service.Orders.OrderChat
{
    public class OrderChatConversationRepository : DataGeneric<OrderChatConversation>, IOrderChatConversationRepository
    {
        public OrderChatConversationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<OrderChatConversation?> GetByOrderIdAsync(int orderId)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.OrderId == orderId && !x.IsDeleted);
        }

        public async Task<OrderChatConversation?> GetByOrderIdTrackingAsync(int orderId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.OrderId == orderId && !x.IsDeleted);
        }

    }
}
