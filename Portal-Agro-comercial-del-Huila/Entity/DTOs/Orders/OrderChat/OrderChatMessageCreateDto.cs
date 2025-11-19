using System.ComponentModel.DataAnnotations;

namespace Entity.DTOs.Orders.OrderChat
{
    public class OrderChatMessageCreateDto
    {
        private const int MaxLength = 2000;

        [Required]
        [MaxLength(MaxLength)]
        public string Message { get; set; } = string.Empty;
    }
}
