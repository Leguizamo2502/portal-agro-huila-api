using Business.Interfaces.Implements.Auth;
using Custom.Encripter;
using Data.Interfaces.Implements.Auth;
using Entity.Domain.Models.Implements.Auth;
using Entity.DTOs.Auth;
using Entity.DTOs.Auth.User;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;
using Utilities.Helpers.Business;
using Utilities.Messaging.Interfaces;

namespace Business.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userData;
        private readonly IRolUserRepository _rolUserData;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;
        private readonly ISendCode _emailService;
        private readonly IPasswordResetCodeRepository _passwordResetRepo;
        private readonly IPersonRepository _personRepository;
        private readonly IEmailVerificationCodeRepository _emailVerificationRepo;
        private readonly ITwoFactorCodeRepository _twoFactorCodeRepository;

        public AuthService(IUserRepository userData,ILogger<AuthService> logger, IRolUserRepository rolUserData, IMapper mapper,
            ISendCode emailService, IPasswordResetCodeRepository passwordResetRepo,IPersonRepository personRepository,
            IEmailVerificationCodeRepository emailVerificationCodeRepository, ITwoFactorCodeRepository twoFactorCodeRepository)
        {
            _logger = logger;
            _userData = userData;
            _rolUserData = rolUserData;
            _mapper = mapper;
            _personRepository = personRepository;
            _emailService = emailService;
            _passwordResetRepo = passwordResetRepo;
            _emailVerificationRepo = emailVerificationCodeRepository;
            _twoFactorCodeRepository = twoFactorCodeRepository;
        }


        public async Task<LoginAttemptResult> PrepareLoginAsync(LoginUserDto dto)
        {
            if (dto == null) throw new ValidationException("Datos inválidos.");
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ValidationException("Las credenciales son obligatorias.");

            dto.Password = EncriptePassword.EncripteSHA256(dto.Password);
            var user = await _userData.LoginUser(dto);

            if (user.IsTwoFactorEnabled)
            {
                await _twoFactorCodeRepository.InvalidateActiveCodesAsync(user.Id);
                var code = new Random().Next(100000, 999999).ToString();

                var twoFactor = new TwoFactorCode
                {
                    UserId = user.Id,
                    Code = code,
                    Expiration = DateTime.UtcNow.AddMinutes(10)
                };

                await _twoFactorCodeRepository.AddAsync(twoFactor);
                await _emailService.SendTwoFactorCodeEmail(user.Email, code);

                return new LoginAttemptResult
                {
                    RequiresTwoFactor = true,
                    User = user
                };
            }

            return new LoginAttemptResult { RequiresTwoFactor = false, User = user };
        }

        public async Task<User> ConfirmTwoFactorLoginAsync(TwoFactorVerificationDto dto)
        {
            if (dto == null) throw new ValidationException("Datos inválidos.");
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
                throw new ValidationException("El correo y el código son obligatorios.");

            var user = await _userData.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado");

            if (!user.IsTwoFactorEnabled)
                throw new UnauthorizedAccessException("La verificación en dos pasos no está activa");

            var record = await _twoFactorCodeRepository.GetValidCodeAsync(user.Id, dto.Code)
                ?? throw new UnauthorizedAccessException("Código inválido o expirado");

            record.IsUsed = true;
            await _twoFactorCodeRepository.UpdateAsync(record);

            return user;
        }

        public async Task UpdateTwoFactorPreferenceAsync(int userId, bool enable)
        {
            var user = await _userData.GetByIdAsync(userId)
                ?? throw new ValidationException("Usuario no encontrado");

            user.IsTwoFactorEnabled = enable;

            await _userData.UpdateAsync(user);

            if (!enable)
                await _twoFactorCodeRepository.InvalidateActiveCodesAsync(userId);
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto, int userId)
        {
            try
            {
                if (dto is null) throw new ValidationException("Datos inválidos.");
                if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                    throw new ValidationException("Las contraseñas no pueden estar vacías.");

                if (!BusinessValidationHelper.IsValidPassword(dto.NewPassword))
                {
                    throw new BusinessException("Contraseña no valida");
                }

                if (dto.NewPassword == dto.CurrentPassword)
                    throw new ValidationException("La nueva contraseña no puede ser igual a la actual.");


                var user = await _userData.GetByIdAsync(userId)
                           ?? throw new ValidationException("Usuario no encontrado.");


                var hashedCurrent = EncriptePassword.EncripteSHA256(dto.CurrentPassword);
                if (!string.Equals(user.Password, hashedCurrent, StringComparison.Ordinal))
                    throw new ValidationException("Credenciales inválidas.");

                // Actualizar nueva contraseña
                user.Password = EncriptePassword.EncripteSHA256(dto.NewPassword);

                await _userData.UpdateAsync(user);


            }
            catch (ValidationException) { throw; }
            catch (Exception ex)
            {
                throw new BusinessException("Error al cambiar la contraseña.", ex);
            }
        }


        public async Task<UserSelectDto?> GetDataBasic(int userId)
        {
            try
            {
                BusinessValidationHelper.ThrowIfZeroOrLess(userId, "El ID debe ser mayor que cero.");

                var entity = await _userData.GetDataBasic(userId);
                var roles = await _rolUserData.GetRolesUserAsync(userId);
                if (entity == null) return null;
                var select = _mapper.Map<UserSelectDto>(entity);
                select.Roles = roles;
                return select;
                
            }
            catch (Exception ex)
            {
                throw new BusinessException($"Error al obtener el registro con ID {userId}.", ex);
            }
        }

        public async Task<IEnumerable<string>> GetRolesUserAsync(int idUser)
        {
            try
            {
                var roles = await _rolUserData.GetRolesUserAsync(idUser);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles del usuario con ID {UserId}", idUser);
                throw new BusinessException("Error al obtener roles del usuario", ex);
            }
        }

        public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                var existingByEmail = await _userData.GetByEmailAsync(dto.Email);
                var existingByDocument = await _userData.GetByDocumentAsync(dto.Identification);

                var existingVerified = existingByEmail?.IsEmailVerified == true
                    || existingByDocument?.IsEmailVerified == true;

                if (existingVerified)
                {
                    if (existingByEmail?.IsEmailVerified == true)
                        throw new Exception("Correo ya registrado");

                    throw new Exception("Ya existe una persona con este numero de identificacion");
                }

                var validPassword = BusinessValidationHelper.IsValidPassword(dto.Password);
                if (!validPassword)
                {
                    throw new BusinessException("Contraseña no valida");
                }

                var person = _mapper.Map<Person>(dto);
                var user = _mapper.Map<User>(dto);

                user.Password = EncriptePassword.EncripteSHA256(user.Password);
                user.IsTwoFactorEnabled = false;

                var existingUser = existingByDocument ?? existingByEmail;

                if (existingUser != null)
                {
                    existingUser.Email = dto.Email;
                    existingUser.Password = user.Password;
                    existingUser.IsDeleted = false;

                    if (existingUser.Person is not null)
                        _mapper.Map(dto, existingUser.Person);
                    else
                        existingUser.Person = person;

                    await _userData.UpdateAsync(existingUser);

                    if (existingUser.RolUsers is null || !existingUser.RolUsers.Any())
                        await _rolUserData.AsignateRolDefault(existingUser);

                    await SendEmailVerificationAsync(existingUser);

                    return _mapper.Map<UserDto>(existingUser);
                }

                user.Person = person;

                user.IsEmailVerified = false;

                await _userData.AddAsync(user);

                await _rolUserData.AsignateRolDefault(user);

                // Recuperar el usuario con sus relaciones para el mapeo correcto
                var createduser = await _userData.GetByIdAsync(user.Id);
                if (createduser == null)
                    throw new BusinessException("Error interno: el usuario no pudo ser recuperado tras la creación.");

                await SendEmailVerificationAsync(createduser);

                return _mapper.Map<UserDto>(createduser);
            }
            catch (Exception ex)
            {
                throw new BusinessException($"Error en el registro del usuario: {ex.Message}", ex);
            }
        }
        private async Task SendEmailVerificationAsync(User user)
        {
            var code = new Random().Next(100000, 999999).ToString();

            var verification = new EmailVerificationCode
            {
                UserId = user.Id,
                Code = code,
                Expiration = DateTime.UtcNow.AddMinutes(10)
            };

            await _emailVerificationRepo.AddAsync(verification);
            await _emailService.SendVerificationCodeEmail(user.Email, code);
        }
        public async Task RequestPasswordResetAsync(string email)
        {
            var user = await _userData.GetByEmailAsync(email)
                ?? throw new ValidationException("Correo no registrado");

            var code = new Random().Next(100000, 999999).ToString();

            var resetCode = new PasswordResetCode
            {
                Email = email,
                Code = code,
                Expiration = DateTime.UtcNow.AddMinutes(10)
            };

            await _passwordResetRepo.AddAsync(resetCode);
            await _emailService.SendRecoveryCodeEmail(email, code);
        }

        public async Task ResetPasswordAsync(ConfirmResetDto dto)
        {
            var record = await _passwordResetRepo.GetValidCodeAsync(dto.Email, dto.Code)
                ?? throw new ValidationException("Código inválido o expirado");

            var user = await _userData.GetByEmailAsync(dto.Email)
                ?? throw new ValidationException("Usuario no encontrado");

            user.Password = EncriptePassword.EncripteSHA256(dto.NewPassword);
            await _userData.UpdateAsync(user);

            record.IsUsed = true;
            await _passwordResetRepo.UpdateAsync(record);
        }

        public async Task RequestEmailVerificationAsync(string email)
        {
            var user = await _userData.GetByEmailAsync(email)
                ?? throw new ValidationException("Correo no registrado");

            if (user.IsEmailVerified)
                throw new ValidationException("El correo ya está verificado");

            await SendEmailVerificationAsync(user);
        }

        public async Task ConfirmEmailVerificationAsync(ConfirmEmailVerificationDto dto)
        {
            var user = await _userData.GetByEmailAsync(dto.Email)
                ?? throw new ValidationException("Usuario no encontrado");

            if (user.IsEmailVerified)
                return;

            var record = await _emailVerificationRepo.GetValidCodeAsync(user.Id, dto.Code)
                ?? throw new ValidationException("Código inválido o expirado");

            user.IsEmailVerified = true;
            await _userData.UpdateAsync(user);

            record.IsUsed = true;
            await _emailVerificationRepo.UpdateAsync(record);
        }
        public async Task<bool> UpdatePerson(PersonUpdateDto dto, int userId)
        {
            try
            {
                var person = await _personRepository.GetByUserIdAsync(userId)
                    ?? throw new ValidationException("Usuario no encontrado");

                _mapper.Map(dto, person);

                await _personRepository.UpdateAsync(person);

                return true;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessException("Error al actualizar la persona.", ex);
            }
        }

    }
}
