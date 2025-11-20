using Business.Interfaces.Implements.Orders.OrderChat;
using Entity.DTOs.Orders.OrderChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Exceptions;
using Utilities.Helpers.Auth;

namespace Web.Controllers.Implements.Orders.OrderChat
{
    [ApiController]
    [Authorize]
    [Route("api/v1/orders/{code}/chat")]
    public class OrderChatController : ControllerBase
    {
        private readonly ILogger<OrderChatController> _logger;
        private readonly IOrderChatService _orderChatService;

        public OrderChatController(
            ILogger<OrderChatController> logger,
            IOrderChatService orderChatService)
        {
            _logger = logger;
            _orderChatService = orderChatService;
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(string code, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var result = await _orderChatService.GetMessagesAsync(userId, code, skip, take);
                return Ok(result);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener mensajes del chat del pedido {Code}", code);
                return StatusCode(500, new { IsSuccess = false, Message = "Error inesperado.", Detail = ex.Message });
            }
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(string code, [FromBody] OrderChatMessageCreateDto dto)
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var message = await _orderChatService.SendMessageAsync(userId, code, dto);
                return Ok(message);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al enviar mensaje en el chat del pedido {Code}", code);
                return StatusCode(500, new { IsSuccess = false, Message = "Error inesperado.", Detail = ex.Message });
            }
        }
    }
}