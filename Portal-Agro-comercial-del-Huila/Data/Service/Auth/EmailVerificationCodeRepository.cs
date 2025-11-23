using Data.Interfaces.Implements.Auth;
using Data.Repository;
using Entity.Domain.Models.Implements.Auth;
using Entity.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Service.Auth
{
    public class EmailVerificationCodeRepository : DataGeneric<EmailVerificationCode>, IEmailVerificationCodeRepository
    {
        public EmailVerificationCodeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<EmailVerificationCode?> GetValidCodeAsync(int userId, string code)
        {
            return _dbSet.FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.Code == code &&
                !c.IsUsed &&
                c.Expiration > DateTime.UtcNow);
        }
    }
}
