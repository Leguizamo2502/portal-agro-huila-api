using Data.Interfaces.Implements.Admin;
using Entity.Domain.Enums;
using Entity.DTOs.Admin;
using Entity.DTOs.Producer.Analytics;
using Entity.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Data.Service.Dashboards
{
    public class AdminDashboardReadRepository : IAdminDashboardReadRepository
    {
        private readonly ApplicationDbContext _context;
        private const int LowStockThreshold = 10;

        public AdminDashboardReadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderFunnelDto> GetOrderFunnelAsync(CancellationToken ct = default)
        {
            var rawBuckets = await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.Active)
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusBucketDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync(ct);

            var bucketsByStatus = rawBuckets.ToDictionary(b => b.Status, StringComparer.OrdinalIgnoreCase);
            var normalizedBuckets = new List<OrderStatusBucketDto>();

            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                if (bucketsByStatus.TryGetValue(status.ToString(), out var bucket))
                {
                    normalizedBuckets.Add(bucket);
                }
                else
                {
                    normalizedBuckets.Add(new OrderStatusBucketDto
                    {
                        Status = status.ToString(),
                        Count = 0
                    });
                }
            }

            return new OrderFunnelDto
            {
                TotalOrders = normalizedBuckets.Sum(b => b.Count),
                Buckets = normalizedBuckets
            };
        }

        public async Task<PaymentSummaryDto> GetPaymentSummaryAsync(CancellationToken ct = default)
        {
            var countsByStatus = await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.Active)
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            int CountFor(params OrderStatus[] statuses)
                => countsByStatus.Where(x => statuses.Contains(x.Status)).Sum(x => x.Count);

            return new PaymentSummaryDto
            {
                PendingAcceptance = CountFor(OrderStatus.PendingReview),
                AwaitingPaymentProof = CountFor(OrderStatus.AcceptedAwaitingPayment),
                PaymentProofPendingReview = CountFor(OrderStatus.PaymentSubmitted),
                ReadyForDelivery = CountFor(OrderStatus.Preparing, OrderStatus.Dispatched),
                CompletedWithProof = CountFor(OrderStatus.DeliveredPendingBuyerConfirm, OrderStatus.Completed)
            };
        }

        public async Task<CatalogSummaryDto> GetCatalogSummaryAsync(CancellationToken ct = default)
        {
            var activeProducers = await _context.Producers
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted && p.Active, ct);

            var totalProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted, ct);

            var publishedProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted && p.Active && p.Status, ct);

            var lowStock = await _context.Products
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted && p.Active && p.Stock <= LowStockThreshold, ct);

            var categories = await _context.Category
                .AsNoTracking()
                .CountAsync(c => !c.IsDeleted && c.Active, ct);

            var favorites = await _context.Favorites
                .AsNoTracking()
                .CountAsync(f => !f.IsDeleted && f.Active, ct);

            return new CatalogSummaryDto
            {
                ActiveProducers = activeProducers,
                TotalProducts = totalProducts,
                PublishedProducts = publishedProducts,
                LowStockProducts = lowStock,
                Categories = categories,
                Favorites = favorites
            };
        }

        public async Task<IEnumerable<TopProducerStatDto>> GetTopProducersAsync(int limit, CancellationToken ct = default)
        {
            limit = NormalizeLimit(limit);

            // 1) Query de agregación pura (todo traducible a SQL)
            var baseQuery =
                from order in _context.Orders.AsNoTracking()
                where !order.IsDeleted
                      && order.Active
                      && order.Status == OrderStatus.Completed
                join producer in _context.Producers.AsNoTracking()
                    on order.ProducerIdSnapshot equals producer.Id
                join user in _context.Users.AsNoTracking()
                    on producer.UserId equals user.Id
                join person in _context.Persons.AsNoTracking()
                    on user.PersonId equals person.Id
                group order by new
                {
                    producer.Id,
                    producer.Description,
                    person.FirstName,
                    person.LastName
                }
                into g
                select new
                {
                    ProducerId = g.Key.Id,
                    Description = g.Key.Description,
                    FirstName = g.Key.FirstName,
                    LastName = g.Key.LastName,
                    CompletedOrders = g.Count(),
                    TotalRevenue = g.Sum(o => o.Total),
                    TotalUnits = g.Sum(o => o.QuantityRequested)
                };

            // 2) Orden + Take (sigue siendo SQL puro)
            var data = await baseQuery
                .OrderByDescending(x => x.TotalRevenue)
                .ThenByDescending(x => x.TotalUnits)
                .Take(limit)
                .ToListAsync(ct);

            // 3) Proyección a DTO en memoria (ya son pocos registros)
            var result = data.Select(x => new TopProducerStatDto
            {
                ProducerId = x.ProducerId,
                ProducerName = string.IsNullOrWhiteSpace(x.Description)
                    ? string.Join(" ", new[] { x.FirstName, x.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    : x.Description,
                CompletedOrders = x.CompletedOrders,
                TotalRevenue = x.TotalRevenue
            });

            return result;
        }

        public async Task<IEnumerable<TopProductStatDto>> GetTopProductsAsync(int limit, CancellationToken ct = default)
        {
            limit = NormalizeLimit(limit);

            var query =
                from order in _context.Orders.AsNoTracking()
                where !order.IsDeleted && order.Active && order.Status == OrderStatus.Completed
                group order by new { order.ProductId, order.ProductNameSnapshot } into g
                orderby g.Sum(x => x.Total) descending, g.Sum(x => x.QuantityRequested) descending
                select new TopProductStatDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductNameSnapshot,
                    CompletedOrders = g.Count(),
                    TotalUnits = g.Sum(x => x.QuantityRequested),
                    TotalRevenue = g.Sum(x => x.Total)
                };

            return await query.Take(limit).ToListAsync(ct);
        }

        private static int NormalizeLimit(int limit)
        {
            if (limit <= 0)
            {
                return 5;
            }

            return limit > 20 ? 20 : limit;
        }
    }
}