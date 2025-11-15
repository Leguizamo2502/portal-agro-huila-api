using Data.Interfaces.Implements.Orders.ConsumerRatings;
using Data.Repository;
using Entity.Domain.Models.Implements.Orders;
using Entity.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Service.Orders.ConsumerRatings
{
    public class ConsumerRatingRepository : DataGeneric<ConsumerRating>, IConsumerRatingRepository
    {
        public ConsumerRatingRepository(ApplicationDbContext context) : base(context)
        {
        }

        private IQueryable<ConsumerRating> BaseQuery()
        {
            return _dbSet
                .AsNoTracking()
                .Include(r => r.User)
                    .ThenInclude(u => u.Person)
                .Include(r => r.Producer)
                .AsSplitQuery();
        }

        public override async Task<IEnumerable<ConsumerRating>> GetAllAsync()
        {
            return await BaseQuery().ToListAsync();
        }

        public override async Task<ConsumerRating?> GetByIdAsync(int id)
        {
            return await BaseQuery().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<ConsumerRating?> GetByOrderIdAsync(int orderId)
        {
            return await BaseQuery().FirstOrDefaultAsync(r => r.OrderId == orderId);
        }

        public async Task<(double? Average, int Count)> GetStatsForUserAsync(int userId)
        {
            var query = _dbSet.AsNoTracking().Where(r => r.UserId == userId && !r.IsDeleted);

            var count = await query.CountAsync();
            if (count == 0)
            {
                return (null, 0);
            }

            var average = await query.AverageAsync(r => (double)r.Rating);
            return (average, count);
        }


    }
}
