using Data.Interfaces.IRepository;
using Entity.Domain.Models.Implements.Auth;

namespace Data.Interfaces.Implements.Auth
{
    public interface ITwoFactorCodeRepository : IDataGeneric<TwoFactorCode>
    {
        Task<TwoFactorCode?> GetValidCodeAsync(int userId, string code);
        Task InvalidateActiveCodesAsync(int userId);
    }
}
