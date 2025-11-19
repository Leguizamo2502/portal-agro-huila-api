namespace Entity.DTOs.Orders.OrderChat
{
    public class OrderChatMessageDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAtUtc { get; set; }
        public int SenderUserId { get; set; }
        public string SenderType { get; set; } = string.Empty;
        public bool IsSystem { get; set; }
        public bool IsMine { get; set; }
    }
}
