using AutoMapper;
using CandidateSearchSystem.Contracts.Interface;
using CandidateSearchSystem.Contracts.Utils;
using CandidateSearchSystem.Data;
using CandidateSearchSystem.Data.DTOs;
using CandidateSearchSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CandidateSearchSystem.Contracts.Service
{
    public class ContactService(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper, ILogger<ContactService> logger) : IContactService
    {
        public async Task<Result<ContactDto, string>> AddAsync(Guid userId, ContactDto dto, CancellationToken token = default)
        {
            try
            {
                // Проверка токена отмены в начале
                token.ThrowIfCancellationRequested();

                await using var context = await contextFactory.CreateDbContextAsync(token);

                // Проверка на null или невалидные данные
                if (dto == null)
                {
                    logger.LogWarning("Попытка добавления контакта с пустым DTO. Пользователь ID: {UserId}", userId);
                    return Result<ContactDto, string>.Failure("DTO контакта не может быть пустым.");
                }

                // Маппинг DTO в сущность и установка UserId
                var contact = mapper.Map<Contact>(dto);
                contact.UserId = userId;

                // Устанавливаем Id как новый (если Id в DTO был заполнен, его игнорируем, если это добавление)
                contact.Id = Guid.NewGuid();

                // Проверка токена отмены перед добавлением в контекст
                token.ThrowIfCancellationRequested();

                await context.Contacts.AddAsync(contact, token);

                // Проверка токена отмены перед сохранением
                token.ThrowIfCancellationRequested();
                await context.SaveChangesAsync(token);

                logger.LogInformation("Контакт успешно добавлен. ID: {ContactId}, Пользователь ID: {UserId}", contact.Id, userId);
                return Result<ContactDto, string>.Success(mapper.Map<ContactDto>(contact));
            }
            catch (OperationCanceledException)
            {
                // Ловим исключение отмены и логируем
                logger.LogWarning("Операция добавления контакта отменена. Пользователь ID: {UserId}", userId);
                return Result<ContactDto, string>.Failure("Операция отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при добавлении контакта. Пользователь ID: {UserId}, DTO: {@Dto}", userId, dto);
                return Result<ContactDto, string>.Failure("Внутренняя ошибка сервера при добавлении контакта.");
            }
        }

        public async Task<EmptyResult> DeleteAsync(Guid userId, Guid contactId, CancellationToken token = default)
        {
            try
            {
                // Проверка токена отмены в начале
                token.ThrowIfCancellationRequested();

                await using var context = await contextFactory.CreateDbContextAsync(token);

                // Проверка на наличие Id
                if (contactId == Guid.Empty)
                {
                    logger.LogWarning("Попытка удаления контакта с пустым DTO или Id. Пользователь ID: {UserId}", userId);
                    return EmptyResult.Failure("Id контакта должен быть указан для удаления.");
                }

                // Находим контакт, убеждаясь, что он принадлежит этому пользователю
                // Токен передается в FirstOrDefaultAsync
                var contact = await context.Contacts
                    .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId, token);

                // Проверка токена отмены после запроса к БД
                token.ThrowIfCancellationRequested();

                if (contact == null)
                {
                    logger.LogWarning("Контакт не найден или не принадлежит пользователю. Контакт ID: {ContactId}, Пользователь ID: {UserId}", contactId, userId);
                    return EmptyResult.Failure("Контакт не найден.");
                }

                context.Contacts.Remove(contact);

                // Проверка токена отмены перед сохранением
                token.ThrowIfCancellationRequested();
                await context.SaveChangesAsync(token);

                logger.LogInformation("Контакт успешно удален. ID: {ContactId}, Пользователь ID: {UserId}", contact.Id, userId);
                return EmptyResult.Success();
            }
            catch (OperationCanceledException)
            {
                // Ловим исключение отмены и логируем
                logger.LogWarning("Операция удаления контакта отменена. Контакт ID: {ContactId}, Пользователь ID: {UserId}", contactId, userId);
                return EmptyResult.Failure("Операция отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при удалении контакта. Контакт ID: {ContactId}, Пользователь ID: {UserId}", contactId, userId);
                return EmptyResult.Failure("Внутренняя ошибка сервера при удалении контакта.");
            }
        }

        public async Task<Result<IEnumerable<ContactDto>, string>> GetByUserIdAsync(Guid userId, CancellationToken token = default)
        {
            try
            {
                // Проверка токена отмены в начале
                token.ThrowIfCancellationRequested();

                await using var context = await contextFactory.CreateDbContextAsync(token);

                // Токен передается в ToListAsync
                var contacts = await context.Contacts
                    .Where(c => c.UserId == userId)
                    .AsNoTracking() // Для чтения лучше использовать AsNoTracking
                    .ToListAsync(token);

                // Проверка токена отмены после запроса к БД
                token.ThrowIfCancellationRequested();

                if (!contacts.Any())
                {
                    logger.LogInformation("Контакты для пользователя не найдены. Пользователь ID: {UserId}", userId);
                    return Result<IEnumerable<ContactDto>, string>.Success(Enumerable.Empty<ContactDto>());
                }

                var contactDtos = mapper.Map<IEnumerable<ContactDto>>(contacts);

                logger.LogInformation("Получено {Count} контактов для пользователя ID: {UserId}", contacts.Count, userId);
                return Result<IEnumerable<ContactDto>, string>.Success(contactDtos);
            }
            catch (OperationCanceledException)
            {
                // Ловим исключение отмены и логируем
                logger.LogWarning("Операция получения контактов отменена. Пользователь ID: {UserId}", userId);
                return Result<IEnumerable<ContactDto>, string>.Failure("Операция отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при получении контактов. Пользователь ID: {UserId}", userId);
                // В случае ошибки возвращаем ошибку с сообщением
                return Result<IEnumerable<ContactDto>, string>.Failure("Внутренняя ошибка сервера при получении контактов.");
            }
        }

        public async Task<Result<ContactDto, string>> UpdateAsync(Guid userId, ContactDto dto, CancellationToken token = default)
        {
            try
            {
                // Проверка токена отмены в начале
                token.ThrowIfCancellationRequested();

                await using var context = await contextFactory.CreateDbContextAsync(token);

                // Проверка на Id
                if (dto == null || dto.Id == Guid.Empty)
                {
                    logger.LogWarning("Попытка обновления контакта с пустым DTO или Id. Пользователь ID: {UserId}", userId);
                    return Result<ContactDto, string>.Failure("Id контакта должен быть указан для обновления.");
                }

                // Находим существующий контакт, убеждаясь, что он принадлежит этому пользователю
                // Токен передается в FirstOrDefaultAsync
                var existingContact = await context.Contacts
                    .FirstOrDefaultAsync(c => c.Id == dto.Id && c.UserId == userId, token);

                // Проверка токена отмены после запроса к БД
                token.ThrowIfCancellationRequested();

                if (existingContact == null)
                {
                    logger.LogWarning("Контакт для обновления не найден или не принадлежит пользователю. Контакт ID: {ContactId}, Пользователь ID: {UserId}", dto.Id, userId);
                    return Result<ContactDto, string>.Failure("Контакт для обновления не найден.");
                }

                // Обновление полей существующего контакта из DTO с помощью AutoMapper
                // AutoMapper по умолчанию копирует поля
                mapper.Map(dto, existingContact);
                // Убедимся, что UserId не изменился (хотя мы его уже проверили в запросе)
                existingContact.UserId = userId;

                // Отмечаем сущность как измененную и сохраняем
                var rez = context.Contacts.Update(existingContact);

                // Проверка токена отмены перед сохранением
                token.ThrowIfCancellationRequested();
                await context.SaveChangesAsync(token);

                logger.LogInformation("Контакт успешно обновлен. ID: {ContactId}, Пользователь ID: {UserId}", existingContact.Id, userId);
                return Result<ContactDto, string>.Success(mapper.Map<ContactDto>(existingContact));
            }
            catch (OperationCanceledException)
            {
                // Ловим исключение отмены и логируем
                logger.LogWarning("Операция обновления контакта отменена. Контакт ID: {ContactId}, Пользователь ID: {UserId}", dto?.Id, userId);
                return Result<ContactDto, string>.Failure("Операция отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обновлении контакта. Контакт ID: {ContactId}, Пользователь ID: {UserId}, DTO: {@Dto}", dto?.Id, userId, dto);
                return Result<ContactDto, string>.Failure("Внутренняя ошибка сервера при обновлении контакта.");
            }
        }
    }
}