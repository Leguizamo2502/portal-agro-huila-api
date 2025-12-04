using Entity.Domain.Models.Implements.Auth;
using Entity.DTOs.Auth;
using Entity.DTOs.Auth.User;

namespace Business.Interfaces.Implements.Auth
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto dto);
        Task RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(ConfirmResetDto dto);
        Task RequestEmailVerificationAsync(string email);
        Task ConfirmEmailVerificationAsync(ConfirmEmailVerificationDto dto);
        Task<IEnumerable<string>> GetRolesUserAsync(int idUser);
        Task<UserSelectDto?> GetDataBasic(int userId);

        Task ChangePasswordAsync(ChangePasswordDto dto, int userId);

        Task<bool> UpdatePerson(PersonUpdateDto dto, int userId);

        Task<LoginAttemptResult> PrepareLoginAsync(LoginUserDto dto);
        Task<User> ConfirmTwoFactorLoginAsync(TwoFactorVerificationDto dto);
        Task UpdateTwoFactorPreferenceAsync(int userId, bool enable);
    }
}
