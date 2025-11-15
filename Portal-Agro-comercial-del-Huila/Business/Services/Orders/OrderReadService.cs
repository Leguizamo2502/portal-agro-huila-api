using Business.Interfaces.Implements.Orders;
using Business.Interfaces.Implements.Orders.ConsumerRatings;
using Data.Interfaces.Implements.Orders;
using Data.Interfaces.Implements.Producers;
using Entity.Domain.Models.Implements.Orders;
using Entity.DTOs.Order.ConsumerRatings;
using Entity.DTOs.Order.ConsumerRatings.Select;
using Entity.DTOs.Order.Select;
using MapsterMapper;
using Utilities.Exceptions;

namespace Business.Services.Orders
{
    public class OrderReadService : IOrderReadService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProducerRepository _producerRepository;
        private readonly IConsumerRatingService _consumerRatingService;
        private readonly IMapper _mapper;
        public OrderReadService(IOrderRepository orderRepository, IProducerRepository producerRepository, IConsumerRatingService consumerRatingService, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _producerRepository = producerRepository;
            _consumerRatingService = consumerRatingService;
            _mapper = mapper;

        }
        public async Task<OrderDetailDto> GetOrderDetailForProducerAsync(int userId, string code)
        {
            var producerId = await _producerRepository.GetIdProducer(userId)
                             ?? throw new BusinessException("El usuario no está registrado como productor.");

            var order = await _orderRepository.GetByCode(code)
                       ?? throw new BusinessException("Orden no encontrada.");

            if (order.IsDeleted || !order.Active)
                throw new BusinessException("La orden no está disponible.");

            if (order.ProducerIdSnapshot != producerId)
                throw new BusinessException("No está autorizado para ver esta orden.");

            var dto = _mapper.Map<OrderDetailDto>(order);
            await EnrichWithCustomerRatingAsync(order, dto);
            return dto;
        }

        /// <summary>
        /// Listar detalle de una orden para un usuario (con snapshot y datos de entrega, sin usuario ni producto completos)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<OrderDetailDto> GetOrderDetailForUserAsync(int userId, string code)
        {
            var order = await _orderRepository.GetByCode(code)
                       ?? throw new BusinessException("Orden no encontrada.");

            if (order.IsDeleted || !order.Active)
                throw new BusinessException("La orden no está disponible.");

            if (order.UserId != userId)
                throw new BusinessException("No está autorizado para ver esta orden.");

            var dto = _mapper.Map<OrderDetailDto>(order);
            await EnrichWithCustomerRatingAsync(order, dto);
            return dto;
        }


        /// <summary>
        /// Listar órdenes de un productor (con snapshot y datos de entrega, sin usuario ni producto completos)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<IEnumerable<OrderListItemDto>> GetOrdersByProducerAsync(int userId)
        {
            var producerId = await _producerRepository.GetIdProducer(userId)
                             ?? throw new BusinessException("El usuario no está registrado como productor.");

            var entities = await _orderRepository.GetOrdersByProducerAsync(producerId);
            return _mapper.Map<IEnumerable<OrderListItemDto>>(entities);
        }


        public async Task<IEnumerable<OrderListItemDto>> GetOrdersByUserAsync(int userId)
        {
            var entities = await _orderRepository.GetOrdersByUserAsync(userId);
            return _mapper.Map<IEnumerable<OrderListItemDto>>(entities);
        }

        /// <summary>
        /// Listar órdenes pendientes de revisión de un productor (con snapshot y datos de entrega, sin usuario ni producto completos)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<IEnumerable<OrderListItemDto>> GetPendingOrdersByProducerAsync(int userId)
        {
            var producerId = await _producerRepository.GetIdProducer(userId)
                             ?? throw new BusinessException("El usuario no está registrado como productor.");

            var entities = await _orderRepository.GetPendingOrdersByProducerAsync(producerId);
            return _mapper.Map<IEnumerable<OrderListItemDto>>(entities);
        }

        private async Task EnrichWithCustomerRatingAsync(Order order, OrderDetailDto dto)
        {
            var stats = await _consumerRatingService.GetCustomerStatsAsync(order.UserId);
            dto.CustomerAverageRating = stats.AverageRating;
            dto.CustomerRatingsCount = stats.RatingsCount;
            dto.ConsumerRating = dto.ConsumerRating ?? _mapper.Map<ConsumerRatingSelectDto?>(order.ConsumerRating);
        }

    }
}