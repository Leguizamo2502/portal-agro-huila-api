using Business.Constants;
using Business.Interfaces.Implements.Notification;
using Business.Interfaces.Implements.Producers.Products;
using Data.Interfaces.Implements.Producers.Products;
using Entity.DTOs.Notifications;
using Microsoft.Extensions.Logging;

namespace Business.Services.Producers.Products
{
    public class LowStockNotifier : ILowStockNotifier
    {
        private readonly IProductRepository _productRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<LowStockNotifier> _logger;

        public LowStockNotifier(
            IProductRepository productRepository,
            INotificationService notificationService,
            ILogger<LowStockNotifier> logger)
        {
            _productRepository = productRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task NotifyIfLowAsync(int productId, int currentStock, CancellationToken ct = default)
        {
            if (currentStock > ProductStockConstants.LowStockThreshold)
                return;

            var product = await _productRepository.GetByIdSmall(productId);
            if (product?.Producer?.User == null)
                return;

            var messageStock = currentStock < 0 ? 0 : currentStock;

            var request = new CreateNotificationRequest
            {
                UserId = product.Producer.UserId,
                Title = "Stock bajo",
                Message = $"Tu producto '{product.Name}' tiene solo {messageStock} unidades disponibles.",
                RelatedType = "Product",
                RelatedRoute = $"/account/producer/management/product"
            };

            try
            {
                await _notificationService.CreateAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo notificar stock bajo para el producto {ProductId}", productId);
            }
        }
    }
}
