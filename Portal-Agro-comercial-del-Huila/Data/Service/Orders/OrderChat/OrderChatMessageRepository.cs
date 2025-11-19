using Data.Interfaces.Implements.Orders.OrderChat;
using Entity.Domain.Models.Implements.Orders.ChatOrder;
using Entity.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Service.Orders.OrderChat
{
    public class OrderChatMessageRepository : IOrderChatMessageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<OrderChatMessage> _dbSet;

        public OrderChatMessageRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<OrderChatMessage>();
        }

        public async Task<OrderChatMessage> AddAsync(OrderChatMessage message)
        {
            _dbSet.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<IReadOnlyList<OrderChatMessage>> GetMessagesAsync(int conversationId, int skip, int take)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.ConversationId == conversationId && !x.IsDeleted)
                .OrderBy(x => x.CreateAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountAsync(int conversationId)
        {
            return await _dbSet.CountAsync(x => x.ConversationId == conversationId && !x.IsDeleted);
        }
    }
}
