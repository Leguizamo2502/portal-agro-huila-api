using Business.Interfaces.Implements.Orders.ConsumerRatings;
using Business.Mapping;
using Business.Services.Orders;
using Data.Interfaces.Implements.Orders;
using Data.Interfaces.Implements.Producers;
using Entity.Domain.Enums;
using Entity.Domain.Models.Implements.Location;
using Entity.Domain.Models.Implements.Orders;
using Entity.DTOs.Order.ConsumerRatings;
using MapsterMapper;
using Moq;
using Utilities.Exceptions;

namespace UnitTest.Modulos.Business.Orders
{
    public class OrderReadServiceTest
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IProducerRepository> _producerRepositoryMock;
        private readonly Mock<IConsumerRatingService> _consumerRatingServiceMock;
        private readonly IMapper _mapper;
        private readonly OrderReadService _service;

        public OrderReadServiceTest()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _producerRepositoryMock = new Mock<IProducerRepository>();
            _consumerRatingServiceMock = new Mock<IConsumerRatingService>();

            var mapperConfig = MapsterConfig.Register();
            _mapper = new Mapper(mapperConfig);

            _service = new OrderReadService(
                _orderRepositoryMock.Object,
                _producerRepositoryMock.Object,
                _consumerRatingServiceMock.Object,
                _mapper);
        }

        

        [Fact]
        public async Task GetOrderDetailForProducerAsync_ShouldThrow_WhenProducerDoesNotOwnOrder()
        {
            const int userId = 9;
            const int producerId = 21;
            var order = BuildOrder(userId: 5, producerIdSnapshot: 99);

            _producerRepositoryMock.Setup(r => r.GetIdProducer(userId)).ReturnsAsync(producerId);
            _orderRepositoryMock.Setup(r => r.GetByCode(order.Code)).ReturnsAsync(order);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.GetOrderDetailForProducerAsync(userId, order.Code));
            Assert.Equal("No está autorizado para ver esta orden.", ex.Message);
        }

        [Fact]
        public async Task GetOrderDetailForUserAsync_ShouldThrow_WhenUserIsNotOwner()
        {
            var order = BuildOrder(userId: 2, producerIdSnapshot: 3);

            _orderRepositoryMock.Setup(r => r.GetByCode(order.Code)).ReturnsAsync(order);

            await Assert.ThrowsAsync<BusinessException>(() => _service.GetOrderDetailForUserAsync(999, order.Code));
        }

        [Fact]
        public async Task GetPendingOrdersByProducerAsync_ShouldReturnMappedList()
        {
            const int userId = 30;
            const int producerId = 77;
            var orders = new List<Order>
            {
                BuildOrder(userId: 1, producerIdSnapshot: producerId, code: "ORD-1"),
                BuildOrder(userId: 2, producerIdSnapshot: producerId, code: "ORD-2", status: OrderStatus.PaymentSubmitted)
            };

            _producerRepositoryMock.Setup(r => r.GetIdProducer(userId)).ReturnsAsync(producerId);
            _orderRepositoryMock.Setup(r => r.GetPendingOrdersByProducerAsync(producerId)).ReturnsAsync(orders);

            var result = await _service.GetPendingOrdersByProducerAsync(userId);

            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Code == "ORD-1");
            Assert.Contains(result, r => r.Status == OrderStatus.PaymentSubmitted.ToString());
        }

        private static Order BuildOrder(
            int userId,
            int producerIdSnapshot,
            string code = "ORD-123",
            OrderStatus status = OrderStatus.PendingReview)
        {
            return new Order
            {
                Id = 1,
                Code = code,
                UserId = userId,
                ProductId = 10,
                ProducerIdSnapshot = producerIdSnapshot,
                ProductNameSnapshot = "Producto de prueba",
                UnitPriceSnapshot = 12.5m,
                QuantityRequested = 3,
                Subtotal = 37.5m,
                Total = 40m,
                Status = status,
                PaymentImageUrl = "http://image.test/pay.png",
                PaymentUploadedAt = DateTime.UtcNow,
                ProducerDecisionAt = DateTime.UtcNow,
                ProducerDecisionReason = "OK",
                ProducerNotes = "Notas",
                RecipientName = "Cliente",
                ContactPhone = "123456",
                AddressLine1 = "Calle 1",
                CityId = 5,
                AdditionalNotes = "Entrega rápida",
                UserReceivedAt = DateTime.UtcNow,
                City = new City
                {
                    Name = "Neiva",
                    Department = new Department { Name = "Huila" }
                },
                ConsumerRating = new ConsumerRating
                {
                    Rating = 5,
                    Comment = "Excelente"
                },
                RowVersion = new byte[] { 1, 2 }
            };
        }
    }
}
