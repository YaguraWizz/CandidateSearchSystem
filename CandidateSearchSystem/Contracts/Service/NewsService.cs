using AutoMapper;
using CandidateSearchSystem.Contracts.Interface;
using CandidateSearchSystem.Contracts.Utils;
using CandidateSearchSystem.Data;
using CandidateSearchSystem.Data.DTOs;
using CandidateSearchSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CandidateSearchSystem.Contracts.Service
{
    public class NewsService(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper, ILogger<NewsService> logger) : INewsService
    {
        public async Task<EmptyResult> AddAsync(Guid userId, NewsPostDto dto, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                await using var context = await contextFactory.CreateDbContextAsync(token);

                var model = mapper.Map<NewsPost>(dto);
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }

                await context.NewsPosts.AddAsync(model, token);
                await context.SaveChangesAsync(token);

                return EmptyResult.Success();
            }
            catch (OperationCanceledException)
            {
                return EmptyResult.Failure("Операция добавления отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddAsync: Ошибка при добавлении новости.");
                return EmptyResult.Failure("Ошибка сервера при добавлении новости.");
            }
        }

        public async Task<EmptyResult> DeleteAsync(Guid userId, NewsPostDto dto, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                await using var context = await contextFactory.CreateDbContextAsync(token);

                // Загружаем сущность
                var existing = await context.NewsPosts
                    .FirstOrDefaultAsync(n => n.Id == userId, token);

                if (existing is null)
                {
                    // Если запись уже удалена/не существует, считаем операцию успешной
                    return EmptyResult.Success();
                }

                context.NewsPosts.Remove(existing);
                await context.SaveChangesAsync(token);

                return EmptyResult.Success();
            }
            catch (OperationCanceledException)
            {
                return EmptyResult.Failure("Операция удаления отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DeleteAsync: Ошибка при удалении новости с ID {userId}", userId);
                return EmptyResult.Failure("Ошибка сервера при удалении новости.");
            }
        }

        public async Task<EmptyResult> UpdateAsync(Guid userId, NewsPostDto dto, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                await using var context = await contextFactory.CreateDbContextAsync(token);

                var existing = await context.NewsPosts
                    .FirstOrDefaultAsync(n => n.Id == dto.Id, token);

                if (existing is null)
                {
                    return EmptyResult.Failure($"Новость с ID {dto.Id} не найдена.");
                }

                // Используем AutoMapper для переноса изменений из DTO в отслеживаемую сущность
                mapper.Map(dto, existing);

                await context.SaveChangesAsync(token);

                return EmptyResult.Success();
            }
            catch (OperationCanceledException)
            {
                return EmptyResult.Failure("Операция обновления отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UpdateAsync: Ошибка при обновлении новости с ID {Id}", dto.Id);
                return EmptyResult.Failure("Ошибка сервера при обновлении новости.");
            }
        }

        public async Task<Result<NewsPostDto, string>> GetByIdAsync(Guid Id, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                await using var context = await contextFactory.CreateDbContextAsync(token);

                var post = await context.NewsPosts.AsNoTracking()
                    .FirstOrDefaultAsync(n => n.Id == Id, token);

                if (post is null)
                {
                    return Result<NewsPostDto, string>.Failure($"Новость с ID {Id} не найдена.");
                }

                return Result<NewsPostDto, string>.Success(mapper.Map<NewsPostDto>(post));
            }
            catch (OperationCanceledException)
            {
                return Result<NewsPostDto, string>.Failure("Операция получения отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetByIdAsync: Ошибка при получении новости с ID {Id}", Id);
                return Result<NewsPostDto, string>.Failure("Ошибка сервера при получении данных.");
            }
        }

        public async Task<Result<Paged<NewsPostDto>, string>> GetNewsPageAsync(int pageIndex, int pageSize, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                await using var context = await contextFactory.CreateDbContextAsync(token);

                // 1. Расчет skip (пропуска)
                if (pageIndex < 1) pageIndex = 1;
                var skip = (pageIndex - 1) * pageSize;

                var query = context.NewsPosts.AsNoTracking();

                // 2. Сортировка (Критически важна для стабильности пагинации!)
                query = query.OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id);

                // 3. Получение общего количества
                var totalCount = await query.CountAsync(token);

                // 4. Пагинация и загрузка элементов
                var items = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(token);

                // 5. Маппинг (используем IMapper)
                var dtoList = mapper.Map<List<NewsPostDto>>(items);

                var pagedResult = new Paged<NewsPostDto>
                {
                    Items = dtoList,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                return Result<Paged<NewsPostDto>, string>.Success(pagedResult);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("GetNewsPageAsync: Операция отменена.");
                return Result<Paged<NewsPostDto>, string>.Failure("Операция отменена.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetNewsPageAsync: Ошибка при получении страницы новостей.");
                return Result<Paged<NewsPostDto>, string>.Failure("Ошибка сервера при получении данных.");
            }
        }

        public async Task<Result<int, string>> GetNewsPageIndexAsync(Guid newsId, int pageSize, CancellationToken token = default)
        {
            await using var context = await contextFactory.CreateDbContextAsync(token);

            try
            {
                token.ThrowIfCancellationRequested();

                // Получаем отсортированный набор Id новостей
                var query = context.NewsPosts.AsNoTracking();

                // 2. Сортировка (Критически важна для стабильности пагинации!)
                query = query.OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id);

                // Получаем позицию новости с указанным GUID
                var allIds = await query.Select(n => n.Id).ToListAsync(token);
                var index = allIds.IndexOf(newsId);

                if (index == -1)
                    return Result<int, string>.Failure($"Новость с таким Guid {newsId} не найдена"); // новость не найдена

                // Вычисляем страницу (1-based)
                int pageIndex = (index / pageSize) + 1;
                return Result<int, string>.Success(pageIndex);
            }
            catch (OperationCanceledException ex)
            {
                // Логируем отмену
                logger.LogWarning(ex, "Операция получения индекса страницы для новости {NewsId} была отменена.", newsId);
                return Result<int, string>.Failure("Операция отменена.");
            }
            catch (Exception ex)
            {
                // Логируем все остальные ошибки
                logger.LogError(ex, "Ошибка при получении индекса страницы для новости {NewsId}", newsId);
                return Result<int, string>.Failure("Ошибка сервера при получении индекса страницы.");
            }
        }
    }
}
