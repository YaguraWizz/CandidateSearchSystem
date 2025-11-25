using AutoMapper;
using CandidateSearchSystem.Contracts.Interface;
using CandidateSearchSystem.Contracts.Utils;
using CandidateSearchSystem.Data.DTOs;
using CandidateSearchSystem.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace CandidateSearchSystem.Contracts.Service
{
    public class AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMapper mapper, ILogger<AccountService> logger) : IAccountService
    {
        public async Task<EmptyResult> LoginAsync(LoginFormDTO dto, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // 1. Поиск пользователя по Email
                var user = await userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                    return EmptyResult.Failure("Неверный Email или пароль.");

                // 2. Попытка входа
                var result = await signInManager.PasswordSignInAsync(
                    user,
                    dto.Password,
                    isPersistent: dto.RememberMe,
                    lockoutOnFailure: true // Включаем блокировку при неудачных попытках
                );

                if (result.Succeeded)
                    return EmptyResult.Success();

                if (result.IsLockedOut)
                    return EmptyResult.Failure("Аккаунт заблокирован из-за большого количества неудачных попыток входа.");

                if (result.IsNotAllowed)
                    return EmptyResult.Failure("Вход не разрешен. Проверьте подтверждение Email.");

                // Общее сообщение для других ошибок (например, неверный пароль)
                return EmptyResult.Failure("Неверный Email или пароль.");
            }
            catch (OperationCanceledException)
            {
                return EmptyResult.Failure("Операция входа отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при попытке входа для пользователя: {Email}", dto.Email);
                return EmptyResult.Failure("Произошла ошибка при попытке входа.");
            }
        }

        public async Task<Result<ApplicationUserDto, string>> RegisterAsync(RegisterFormDTO dto, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // 1. Проверка существования пользователя по Email
                if (await userManager.FindByEmailAsync(dto.Email) != null)
                {
                    return Result<ApplicationUserDto, string>.Failure("Пользователь с таким Email уже зарегистрирован.");
                }

                // 2. Маппинг DTO -> Model
                var user = mapper.Map<ApplicationUser>(dto);

                // 3. Создание пользователя
                var result = await userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result<ApplicationUserDto, string>.Failure($"Ошибка при регистрации: {errors}");
                }

                // 4. Возвращаем DTO созданного пользователя
                return Result<ApplicationUserDto, string>.Success(mapper.Map<ApplicationUserDto>(user));
            }
            catch (OperationCanceledException)
            {
                return Result<ApplicationUserDto, string>.Failure("Операция регистрации отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при регистрации пользователя с Email: {Email}", dto.Email);
                return Result<ApplicationUserDto, string>.Failure("Произошла ошибка сервера при регистрации.");
            }
        }

        public async Task<EmptyResult> LogoutAsync()
        {
            try
            {
                await signInManager.SignOutAsync();
                return EmptyResult.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при выходе пользователя.");
                return EmptyResult.Failure("Произошла ошибка при попытке выхода.");
            }
        }

        public async Task<Result<ApplicationUserDto, string>> GetByIdAsync(Guid Id, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var user = await userManager.FindByIdAsync(Id.ToString());

                if (user == null)
                {
                    return Result<ApplicationUserDto, string>.Failure("Пользователь не найден.");
                }

                return Result<ApplicationUserDto, string>.Success(mapper.Map<ApplicationUserDto>(user));
            }
            catch (OperationCanceledException)
            {
                return Result<ApplicationUserDto, string>.Failure("Операция получения данных отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при получении пользователя по ID: {Id}", Id);
                return Result<ApplicationUserDto, string>.Failure("Произошла ошибка сервера при получении данных пользователя.");
            }
        }

        public async Task<Result<ApplicationUserDto, string>> UpdateAsync(Guid Id, ApplicationUserEditDto dto, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var user = await userManager.FindByIdAsync(Id.ToString());
                if (user == null)
                {
                    return Result<ApplicationUserDto, string>.Failure("Пользователь для обновления не найден.");
                }

                // Маппим DTO на существующую сущность (обновляем только разрешенные поля)
                mapper.Map(dto, user);

                // Обновление пользователя (сохранение изменений в БД)
                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result<ApplicationUserDto, string>.Failure($"Ошибка при обновлении профиля: {errors}");
                }

                // Возвращаем обновленный DTO
                return Result<ApplicationUserDto, string>.Success(mapper.Map<ApplicationUserDto>(user));
            }
            catch (OperationCanceledException)
            {
                return Result<ApplicationUserDto, string>.Failure("Операция обновления отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обновлении профиля пользователя ID: {Id}", Id);
                return Result<ApplicationUserDto, string>.Failure("Произошла ошибка сервера при обновлении профиля.");
            }
        }

        public async Task<EmptyResult> ChangePasswordAsync(Guid Id, ChangePasswordDto dto, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(dto.CurrentPassword, nameof(dto.CurrentPassword));
            ArgumentNullException.ThrowIfNull(dto.NewPassword, nameof(dto.NewPassword));

            try
            {
                token.ThrowIfCancellationRequested();

                var user = await userManager.FindByIdAsync(Id.ToString());
                if (user == null)
                {
                    return EmptyResult.Failure("Пользователь не найден.");
                }

                // Выполняем смену пароля с проверкой текущего пароля
                var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

                if (!result.Succeeded)
                {
                    // Обработка специфических ошибок, например, "Incorrect password"
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));

                    if (errors.Contains("Incorrect password"))
                    {
                        return EmptyResult.Failure("Неверный текущий пароль.");
                    }

                    return EmptyResult.Failure($"Ошибка при смене пароля: {errors}");
                }

                logger.LogInformation("Пользователь ID: {Id} успешно сменил пароль.", Id);
                return EmptyResult.Success();
            }
            catch (OperationCanceledException)
            {
                return EmptyResult.Failure("Операция смены пароля отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при смене пароля пользователя ID: {Id}", Id);
                return EmptyResult.Failure("Произошла ошибка сервера при смене пароля.");
            }
        }

        public async Task<EmptyResult> DeleteAsync(Guid Id, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var user = await userManager.FindByIdAsync(Id.ToString());
                if (user == null)
                {
                    // Если пользователя нет, считаем операцию успешной
                    return EmptyResult.Success();
                }

                // 1. Устанавливаем флаги мягкого удаления
                // Предполагается, что ваша модель ApplicationUser имеет свойства IsDeleted и DeletedAt
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                // Опционально: Очищаем поля, связанные с безопасностью, чтобы предотвратить повторный вход
                user.EmailConfirmed = false;
                user.LockoutEnd = DateTimeOffset.MaxValue; // Блокировка аккаунта
                user.AccessFailedCount = 0;

                // 2. Вместо DeleteAsync используем UpdateAsync для сохранения изменений
                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return EmptyResult.Failure($"Ошибка при мягком удалении пользователя: {errors}");
                }

                // 3. Также важно, чтобы при мягком удалении пользователь был разлогинен
                await signInManager.SignOutAsync();

                return EmptyResult.Success();
            }
            catch (OperationCanceledException)
            {
                return EmptyResult.Failure("Операция удаления отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при мягком удалении пользователя ID: {Id}", Id);
                return EmptyResult.Failure("Произошла ошибка сервера при удалении пользователя.");
            }
        }

    }
}
