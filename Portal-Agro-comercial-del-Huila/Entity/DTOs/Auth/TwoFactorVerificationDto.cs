namespace Entity.DTOs.Auth
{
    public class TwoFactorVerificationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
