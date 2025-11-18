using Business.Interfaces.Implements.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Implements.Admin
{
    [ApiController]
    [Route("api/v1/admin/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IAdminDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOverview([FromQuery] int? top, CancellationToken ct)
        {
            try
            {
                var limit = top.GetValueOrDefault(5);
                var snapshot = await _dashboardService.GetDashboardSnapshotAsync(limit, ct);
                return Ok(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al construir la vista general del dashboard.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor." });
            }
        }

        [HttpGet("orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderFunnel(CancellationToken ct)
        {
            try
            {
                var funnel = await _dashboardService.GetOrderFunnelAsync(ct);
                return Ok(funnel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el funnel de pedidos.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor." });
            }
        }

        [HttpGet("payments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentSummary(CancellationToken ct)
        {
            try
            {
                var payments = await _dashboardService.GetPaymentSummaryAsync(ct);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el resumen de pagos.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor." });
            }
        }

        [HttpGet("catalog")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCatalogSummary(CancellationToken ct)
        {
            try
            {
                var catalog = await _dashboardService.GetCatalogSummaryAsync(ct);
                return Ok(catalog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la salud del catálogo.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor." });
            }
        }

        [HttpGet("top-producers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTopProducers([FromQuery] int? limit, CancellationToken ct)
        {
            try
            {
                var safeLimit = limit.GetValueOrDefault(5);
                var topProducers = await _dashboardService.GetTopProducersAsync(safeLimit, ct);
                return Ok(new { items = topProducers, total = topProducers.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el ranking de productores.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor." });
            }
        }

        [HttpGet("top-products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTopProducts([FromQuery] int? limit, CancellationToken ct)
        {
            try
            {
                var safeLimit = limit.GetValueOrDefault(5);
                var topProducts = await _dashboardService.GetTopProductsAsync(safeLimit, ct);
                return Ok(new { items = topProducts, total = topProducts.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el ranking de productos.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor." });
            }
        }
    }
}