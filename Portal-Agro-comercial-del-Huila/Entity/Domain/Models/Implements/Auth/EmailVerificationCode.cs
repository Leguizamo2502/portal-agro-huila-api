using Entity.Domain.Models.Base;

namespace Entity.Domain.Models.Implements.Auth
{
    public class EmailVerificationCode : BaseModel
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; }
    }
}
