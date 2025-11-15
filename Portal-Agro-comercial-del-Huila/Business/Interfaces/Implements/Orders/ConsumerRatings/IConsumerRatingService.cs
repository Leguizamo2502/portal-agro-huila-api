using Entity.DTOs.Order.ConsumerRatings;
using Entity.DTOs.Order.ConsumerRatings.Create;
using Entity.DTOs.Order.ConsumerRatings.Select;

namespace Business.Interfaces.Implements.Orders.ConsumerRatings
{
    public interface IConsumerRatingService
    {
        Task<ConsumerRatingSelectDto> RateCustomerAsync(int producerUserId, string orderCode, ConsumerRatingCreateDto dto);
        Task<ConsumerRatingSelectDto?> GetRatingForOrderAsync(int producerUserId, string orderCode);
        Task<ConsumerRatingStatsDto> GetCustomerStatsAsync(int userId);
    }
}
