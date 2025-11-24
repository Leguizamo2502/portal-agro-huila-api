using Business.Interfaces.Implements.Notification;
using Business.Interfaces.Implements.Orders.OrderChat;
using Business.Interfaces.Implements.Producers.Cloudinary;
using Business.Interfaces.Implements.Producers.Products;
using Business.Services.Orders;
using Data.Interfaces.Implements.Auth;
using Data.Interfaces.Implements.Orders;
using Data.Interfaces.Implements.Producers;
using Data.Interfaces.Implements.Producers.Products;
using Entity.Domain.Enums;
using Entity.Domain.Models.Implements.Orders;
using Entity.Domain.Models.Implements.Producers;
using Entity.Domain.Models.Implements.Producers.Products;
using Entity.DTOs.Notifications;
using Entity.DTOs.Order.Create;
using Entity.Infrastructure.Context;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Exceptions;
using Utilities.Messaging.Interfaces;

namespace UnitTest.Modulos.Business.Orders
{
    public class OrderServiceTest
    {
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<OrderService>> _loggerMock = new();
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock = new();
        private readonly Mock<IProductRepository> _productRepositoryMock = new();
        private readonly Mock<IOrderEmailService> _orderEmailServiceMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IProducerRepository> _producerRepositoryMock = new();
        private readonly Mock<IOrderChatService> _orderChatServiceMock = new();
        private readonly Mock<ILowStockNotifier> _lowStockNotifierMock = new();
        private readonly Mock<ApplicationDbContext> _dbContextMock;
        private readonly OrderService _service;

        public OrderServiceTest()
        {
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            _dbContextMock = new Mock<ApplicationDbContext>(dbOptions);

            _service = new OrderService(
                _mapperMock.Object,
                _loggerMock.Object,
                _orderRepositoryMock.Object,
                _notificationServiceMock.Object,
                _cloudinaryServiceMock.Object,
                _productRepositoryMock.Object,
                _dbContextMock.Object,
                _orderEmailServiceMock.Object,
                _userRepositoryMock.Object,
                _producerRepositoryMock.Object,
                _orderChatServiceMock.Object,
                _lowStockNotifierMock.Object,
                cfg);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrow_WhenUserBuysOwnProduct()
        {
            const int userId = 10;
            var dto = BuildValidDto();
            var product = BuildActiveProduct(producerUserId: userId, stock: 10);

            _productRepositoryMock.Setup(r => r.GetByIdAsync(dto.ProductId)).ReturnsAsync(product);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateOrderAsync(userId, dto));

            Assert.Equal("No puedes comprar tus propios productos.", ex.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrow_WhenStockInsufficient()
        {
            const int userId = 5;
            var dto = BuildValidDto(quantity: 6);
            var product = BuildActiveProduct(stock: 3);

            _productRepositoryMock.Setup(r => r.GetByIdAsync(dto.ProductId)).ReturnsAsync(product);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateOrderAsync(userId, dto));

            Assert.Equal("Stock insuficiente para la cantidad solicitada.", ex.Message);
        }


        private static OrderCreateDto BuildValidDto(int quantity = 2, string recipientName = "Cliente", string notes = "Notas")
        {
            return new OrderCreateDto
            {
                ProductId = 1,
                QuantityRequested = quantity,
                RecipientName = recipientName,
                ContactPhone = "123456",
                AddressLine1 = "Calle 1",
                AddressLine2 = "",
                CityId = 10,
                AdditionalNotes = notes
            };
        }

        private static Product BuildActiveProduct(int producerUserId = 7, int stock = 5, decimal price = 15m)
        {
            return new Product
            {
                Id = 1,
                Active = true,
                IsDeleted = false,
                Stock = stock,
                Price = price,
                ProducerId = 3,
                Producer = new Producer
                {
                    Id = 3,
                    UserId = producerUserId
                }
            };
        }
    }
}
