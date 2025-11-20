using Business.Interfaces.Implements.Notification;
using Business.Interfaces.Implements.Orders.ConsumerRatings;
using Data.Interfaces.Implements.Orders;
using Data.Interfaces.Implements.Orders.ConsumerRatings;
using Data.Interfaces.Implements.Producers;
using Entity.Domain.Enums;
using Entity.Domain.Models.Implements.Orders;
using Entity.DTOs.Notifications;
using Entity.DTOs.Order.ConsumerRatings;
using Entity.DTOs.Order.ConsumerRatings.Create;
using Entity.DTOs.Order.ConsumerRatings.Select;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business.Services.Orders.ConsumerRatings
{
    public class ConsumerRatingService : IConsumerRatingService
    {
        private readonly IConsumerRatingRepository _ratingRepository;
        private readonly IProducerRepository _producerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsumerRatingService> _logger;

        public ConsumerRatingService(
            IConsumerRatingRepository ratingRepository,
            IProducerRepository producerRepository,
            IOrderRepository orderRepository,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<ConsumerRatingService> logger)
        {
            _ratingRepository = ratingRepository;
            _producerRepository = producerRepository;
            _orderRepository = orderRepository;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ConsumerRatingSelectDto> RateCustomerAsync(int producerUserId, string orderCode, ConsumerRatingCreateDto dto)
        {
            ValidateRating(dto);

            var producerId = await _producerRepository.GetIdProducer(producerUserId)
                ?? throw new BusinessException("El usuario no está registrado como productor.");

            var order = await _orderRepository.GetByCode(orderCode)
                ?? throw new BusinessException("Orden no encontrada.");

            if (order.ProducerIdSnapshot != producerId)
            {
                throw new BusinessException("No está autorizado para calificar esta orden.");
            }

            if (order.Status != OrderStatus.Completed)
            {
                throw new BusinessException("Solo se pueden calificar órdenes completadas.");
            }

            if (order.ConsumerRating is not null && !order.ConsumerRating.IsDeleted)
            {
                throw new BusinessException("La orden ya fue calificada.");
            }

            var providedRowVersion = ParseRowVersion(dto.RowVersion);
            var currentRowVersion = order.RowVersion ?? Array.Empty<byte>();
            if (!currentRowVersion.SequenceEqual(providedRowVersion))
            {
                throw new BusinessException("La orden fue modificada. Actualiza la página e inténtalo nuevamente.");
            }

            var entity = new ConsumerRating
            {
                OrderId = order.Id,
                ProducerId = producerId,
                UserId = order.UserId,
                Rating = dto.Rating,
                Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim()
            };

            try
            {
                var created = await _ratingRepository.AddAsync(entity);
                var reloaded = await _ratingRepository.GetByIdAsync(created.Id)
                    ?? throw new BusinessException("No se pudo cargar la calificación creada.");
                await _notificationService.CreateAsync(new CreateNotificationRequest
                {
                    UserId = order.UserId,
                    Title = "Has recibido una calificación",
                    Message = $"El productor ha calificado tu compra con {dto.Rating} estrella(s).",
                    RelatedType = "Order",
                    RelatedRoute = $"/account/orders/{order.Code}"
                });

                return _mapper.Map<ConsumerRatingSelectDto>(reloaded);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al registrar calificación para la orden {OrderCode}", orderCode);
                throw new BusinessException("Error al registrar la calificación del cliente.", ex);
            }
        }

        public async Task<ConsumerRatingSelectDto?> GetRatingForOrderAsync(int producerUserId, string orderCode)
        {
            var producerId = await _producerRepository.GetIdProducer(producerUserId)
                ?? throw new BusinessException("El usuario no está registrado como productor.");

            var order = await _orderRepository.GetByCode(orderCode)
                ?? throw new BusinessException("Orden no encontrada.");

            if (order.ProducerIdSnapshot != producerId)
            {
                throw new BusinessException("No está autorizado para consultar esta orden.");
            }

            var rating = await _ratingRepository.GetByOrderIdAsync(order.Id);
            return rating is null ? null : _mapper.Map<ConsumerRatingSelectDto>(rating);
        }

        public async Task<ConsumerRatingStatsDto> GetCustomerStatsAsync(int userId)
        {
            var (average, count) = await _ratingRepository.GetStatsForUserAsync(userId);
            return new ConsumerRatingStatsDto
            {
                AverageRating = average,
                RatingsCount = count
            };
        }

        private static void ValidateRating(ConsumerRatingCreateDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                throw new BusinessException("La calificación debe estar entre 1 y 5.");
            }
        }

        private static byte[] ParseRowVersion(string rowVersionBase64)
        {
            if (string.IsNullOrWhiteSpace(rowVersionBase64))
            {
                throw new BusinessException("RowVersion es requerido.");
            }

            try
            {
                return Convert.FromBase64String(rowVersionBase64);
            }
            catch (FormatException)
            {
                throw new BusinessException("RowVersion inválido.");
            }
        }
    }
}