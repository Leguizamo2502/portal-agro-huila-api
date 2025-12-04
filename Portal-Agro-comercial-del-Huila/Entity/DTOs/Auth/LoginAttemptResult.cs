namespace Entity.DTOs.Auth
{
    public class LoginAttemptResult
    {
        public bool RequiresTwoFactor { get; set; }
        public Entity.Domain.Models.Implements.Auth.User User { get; set; } = null!;
    }
}