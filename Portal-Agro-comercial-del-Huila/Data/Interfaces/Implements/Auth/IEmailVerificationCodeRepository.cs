using Data.Interfaces.IRepository;
using Entity.Domain.Models.Implements.Auth;

namespace Data.Interfaces.Implements.Auth
{
    public interface IEmailVerificationCodeRepository : IDataGeneric<EmailVerificationCode>
    {
        Task<EmailVerificationCode?> GetValidCodeAsync(int userId, string code);
    }
}
