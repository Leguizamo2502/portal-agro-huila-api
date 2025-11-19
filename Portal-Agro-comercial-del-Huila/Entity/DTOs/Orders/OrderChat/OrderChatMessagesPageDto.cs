namespace Entity.DTOs.Orders.OrderChat
{
    public class OrderChatMessagesPageDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int ConversationId { get; set; }
        public int Total { get; set; }
        public bool HasMore { get; set; }
        public bool IsChatEnabled { get; set; }
        public bool IsChatClosed { get; set; }
        public bool CanSendMessages { get; set; }
        public string? ChatDisabledReason { get; set; }
        public string? ChatClosedReason { get; set; }
        public DateTime? ChatEnabledAt { get; set; }
        public DateTime? ChatClosedAt { get; set; }
        public IReadOnlyCollection<OrderChatMessageDto> Messages { get; set; } = Array.Empty<OrderChatMessageDto>();
    }
}
