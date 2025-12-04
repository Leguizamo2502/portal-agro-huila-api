using Data.Interfaces.Implements.Auth;
using Data.Repository;
using Entity.Domain.Models.Implements.Auth;
using Entity.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Service.Auth
{
    public class TwoFactorCodeRepository : DataGeneric<TwoFactorCode>, ITwoFactorCodeRepository
    {
        public TwoFactorCodeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<TwoFactorCode?> GetValidCodeAsync(int userId, string code)
        {
            return _dbSet.FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.Code == code &&
                !c.IsUsed &&
                c.Expiration > DateTime.UtcNow);
        }

        public async Task InvalidateActiveCodesAsync(int userId)
        {
            var activeCodes = await _dbSet
                .Where(c => c.UserId == userId && !c.IsDeleted && !c.IsUsed && c.Expiration > DateTime.UtcNow)
                .ToListAsync();

            if (activeCodes.Count == 0)
                return;

            foreach (var code in activeCodes)
            {
                code.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
