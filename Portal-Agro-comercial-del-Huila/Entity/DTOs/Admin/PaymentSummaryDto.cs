namespace Entity.DTOs.Admin
{
    public class PaymentSummaryDto
    {
        public int PendingAcceptance { get; set; }
        public int AwaitingPaymentProof { get; set; }
        public int PaymentProofPendingReview { get; set; }
        public int ReadyForDelivery { get; set; }
        public int CompletedWithProof { get; set; }
    }
}
