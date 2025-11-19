namespace Entity.DTOs.Orders.OrderChat
{
    public class OrderChatMessagesPageDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int ConversationId { get; set; }
        public int Total { get; set; }
        public bool HasMore { get; set; }
        public IReadOnlyCollection<OrderChatMessageDto> Messages { get; set; } = Array.Empty<OrderChatMessageDto>();
    }
}
