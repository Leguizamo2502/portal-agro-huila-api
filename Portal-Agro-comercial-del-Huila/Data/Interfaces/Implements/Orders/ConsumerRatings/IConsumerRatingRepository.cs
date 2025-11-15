using Data.Interfaces.IRepository;
using Entity.Domain.Models.Implements.Orders;

namespace Data.Interfaces.Implements.Orders.ConsumerRatings
{
    public interface IConsumerRatingRepository : IDataGeneric<ConsumerRating>
    {
        Task<ConsumerRating?> GetByOrderIdAsync(int orderId);
        Task<(double? Average, int Count)> GetStatsForUserAsync(int userId);
    }
}
