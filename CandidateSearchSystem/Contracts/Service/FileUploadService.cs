using AutoMapper;
using CandidateSearchSystem.Contracts.Interface;
using CandidateSearchSystem.Contracts.Utils;
using CandidateSearchSystem.Data;
using CandidateSearchSystem.Data.Constants;
using CandidateSearchSystem.Data.DTOs;
using CandidateSearchSystem.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CandidateSearchSystem.Contracts.Service
{
    public class FileUploadSettings
    {
        // Установлен разумный лимит по умолчанию (5 MB)
        public int MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5 MB
        public Dictionary<string, string[]> AllowedExtensions { get; set; } = [];
    }

    // Изменен ILogger<ContactService> на ILogger<FileUploadService>
    public class FileUploadService(IDbContextFactory<ApplicationDbContext> _contextFactory, IOptions<FileUploadSettings> options,
        IWebHostEnvironment _environment, IMapper _mapper, ILogger<FileUploadService> _logger) : IFileService
    {
        private readonly string _uploadBasePath = "Uploads";
        private readonly FileUploadSettings _settings = options.Value;

        /// <summary>
        /// Формирует полный путь к каталогу загрузок конкретного пользователя: [WebRootPath]/Uploads/[userId]
        /// </summary>
        private string GetUserUploadDirectory(Guid userId)
        {
            // Используем WebRootPath для пути, который будет доступен через HTTP (wwwroot/)
            var baseDir = Path.Combine(_environment.WebRootPath, _uploadBasePath);
            return Path.Combine(baseDir, userId.ToString());
        }

        /// <summary>
        /// Проверяет и при необходимости создает каталог загрузок для конкретного пользователя.
        /// </summary>
        private void CheckUserUploadDirectory(Guid userId)
        {
            var uploadDirectory = GetUserUploadDirectory(userId);

            if (!Directory.Exists(uploadDirectory))
            {
                _logger.LogInformation("Каталог загрузки пользователя '{UploadDir}' не найден. Создание...", uploadDirectory);
                try
                {
                    // Создание каталога, включая все промежуточные
                    Directory.CreateDirectory(uploadDirectory);
                    _logger.LogInformation("Каталог успешно создан. Путь до каталога: {uploadDirectory}", uploadDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании каталога загрузки пользователя '{UploadDir}'.", uploadDirectory);
                    throw; // Критическая ошибка
                }
            }
        }

        private bool IsExtensionAllowed(string fileExtension)
        {
            // Приводим к нижнему регистру для надежного сравнения
            var normalizedExtension = fileExtension.ToLowerInvariant();

            // Если расширение файла не содержит точку, добавляем ее
            if (!normalizedExtension.StartsWith("."))
            {
                normalizedExtension = "." + normalizedExtension;
            }

            foreach (var allowedExts in _settings.AllowedExtensions.Values)
            {
                // Проверяем, есть ли нормализованное расширение в списке разрешенных
                if (allowedExts.Contains(normalizedExtension, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Сохраняет файл, привязывает его к пользователю и сохраняет метаданные.
        /// </summary>
        // Изменена сигнатура для принятия метаданных
        public async Task<Result<FileDto, string>> AddAsync(Guid userId, IBrowserFile file,
            FileCreationDto metadata, CancellationToken token = default)
        {
            try
            {
                // 1. Предварительные проверки
                var fileExtension = Path.GetExtension(file.Name).ToLowerInvariant();

                if (_settings.MaxFileSize > 0 && file.Size > _settings.MaxFileSize)
                {
                    _logger.LogWarning("Файл '{FileName}' превышает максимальный размер.", file.Name);
                    return Result<FileDto, string>.Failure($"File: {file.Name} exceeds the maximum allowed file size ({_settings.MaxFileSize} bytes).");
                }

                if (!IsExtensionAllowed(fileExtension))
                {
                    _logger.LogWarning("Тип файла '{FileName}' ({FileExtension}) не разрешен.", file.Name, fileExtension);
                    return Result<FileDto, string>.Failure($"File: {file.Name}, File type not allowed ({fileExtension})");
                }

                // 2. Генерация ID и формирование пути (upload/userid/fileid)
                var fileId = Guid.NewGuid();
                // Путь, который будет храниться в БД (относительный от wwwroot)
                var relativePath = Path.Combine(_uploadBasePath, userId.ToString(), $"{fileId}{fileExtension}");
                var storagePath = relativePath.Replace('\\', '/'); // Для URL

                // 3. Сохранение файла на диске
                CheckUserUploadDirectory(userId);
                await SaveFileAsync(userId, fileId, file, fileExtension, _settings.MaxFileSize, token);

                // 4. Создание DTO с использованием метаданных
                Files modelEntity = new()
                {
                    Id = fileId,
                    UserId = userId,                    // Привязка к пользователю
                    Name = file.Name,                   // Имя из IBrowserFile
                    Type = metadata.Type,               // Метаданные из DTO
                    Description = metadata.Description, // Метаданные из DTO
                    StoragePath = storagePath,          // Сгенерированный путь
                    UploadedAt = DateTimeOffset.UtcNow
                };

                // 5. Сохранение записи в базе данных
                await using var context = await _contextFactory.CreateDbContextAsync(token);

                context.Files.Add(modelEntity);
                await context.SaveChangesAsync(token);

                _logger.LogInformation("Файл '{FileName}' успешно загружен и сохранен с ID '{FileId}' для пользователя {UserId}.", file.Name, fileId, userId);

                return Result<FileDto, string>.Success(_mapper.Map<FileDto>(modelEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при загрузке файла для пользователя {UserId}.", userId);
                return Result<FileDto, string>.Failure($"An error occurred during file upload: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохраняет файл на диске по пути [WebRootPath]/Uploads/[userId]/[fileId].[ext]
        /// </summary>
        private async Task SaveFileAsync(Guid userId, Guid fileId, IBrowserFile file, string fileExtension, int maxFileSize, CancellationToken token)
        {
            var userDirectory = GetUserUploadDirectory(userId);
            var fileName = $"{fileId}{fileExtension}";
            var path = Path.Combine(userDirectory, fileName);

            try
            {
                // Улучшенный FileStream с использованием асинхронного доступа
                await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

                // OpenReadStream проверяет MaxFileSize
                await file.OpenReadStream(maxFileSize, token).CopyToAsync(fs, token);

                _logger.LogDebug("Файл {FileName} сохранен по пути {Path}.", file.Name, path);
            }
            catch (Exception ex) when (ex is IOException || ex is OperationCanceledException)
            {
                _logger.LogError(ex, "Ошибка при сохранении файла {FileName} по пути {Path}.", file.Name, path);
                throw;
            }
        }

        public async Task<EmptyResult> DeleteAsync(Guid userId, FileDto dto, CancellationToken token = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(token);

            // Проверка владения файлом
            var fileEntity = await context.Files
                .Where(f => f.Id == dto.Id)
                .Where(f => f.UserId == userId)
                .FirstOrDefaultAsync(token);

            if (fileEntity == null)
            {
                _logger.LogWarning("Попытка удаления несуществующего или чужого файла с ID {FileId} для пользователя {UserId}.", dto.Id, userId);
                return EmptyResult.Failure("File not found or access denied.");
            }

            // Мягкое удаление (soft delete)
            if (!fileEntity.IsDeleted)
            {
                fileEntity.IsDeleted = true;
                fileEntity.DeletedAt = DateTimeOffset.UtcNow;

                // Фактическое удаление файла с диска
                try
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, fileEntity.StoragePath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        _logger.LogInformation("Файл {FileId} удален с диска.", dto.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Файл {FileId} не найден на диске по пути {Path}, выполнено только мягкое удаление в БД.", dto.Id, fullPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении файла с диска для ID {FileId}.", dto.Id);
                    // Продолжаем сохранение в БД, но возвращаем ошибку, если это критично.
                    // Здесь мы просто логируем и продолжаем, т.к. мягкое удаление в БД важнее.
                }

                await context.SaveChangesAsync(token);
            }

            return EmptyResult.Success();
        }

        public async Task<Result<IEnumerable<FileDto>, string>> GetByUserIdAsync(Guid userId, CancellationToken token = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(token);

            try
            {
                // Фильтруем по UserId и исключаем мягко удаленные файлы
                var userFiles = await context.Files
                    .Where(f => f.UserId == userId)
                    .Where(f => !f.IsDeleted)
                    .ToListAsync(token);

                var dtos = _mapper.Map<IEnumerable<FileDto>>(userFiles);

                return Result<IEnumerable<FileDto>, string>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении файлов для пользователя {UserId}.", userId);
                return Result<IEnumerable<FileDto>, string>.Failure($"Error retrieving files: {ex.Message}");
            }
        }

        // FileUploadService.cs

        public async Task<Result<FileDto, string>> UpdateAsync(Guid userId, FileDto dto, CancellationToken token = default)
        {
            // Проверка, что DTO содержит ID файла
            if (dto.Id == Guid.Empty)
            {
                _logger.LogWarning("Попытка обновления метаданных без указания FileId для пользователя {UserId}.", userId);
                return Result<FileDto, string>.Failure("File ID must be provided for update.");
            }

            await using var context = await _contextFactory.CreateDbContextAsync(token);

            try
            {
                // 1. Найти существующую сущность файла в БД
                // Фильтруем по ID файла, ID пользователя (для проверки владения) и исключаем удаленные файлы.
                var fileEntity = await context.Files
                    .Where(f => f.Id == dto.Id)
                    .Where(f => f.UserId == userId)
                    .Where(f => !f.IsDeleted)
                    .FirstOrDefaultAsync(token);

                if (fileEntity == null)
                {
                    _logger.LogWarning("Попытка обновления несуществующего, удаленного или чужого файла с ID {FileId} для пользователя {UserId}.", dto.Id, userId);
                    return Result<FileDto, string>.Failure("File not found or access denied.");
                }

                // 2. Обновить метаданные

                // Обновляем только те поля, которые разрешено менять через этот метод:
                if (!string.IsNullOrEmpty(dto.Name))
                {
                    fileEntity.Name = dto.Name;
                }

                if (!string.IsNullOrEmpty(dto.Description))
                {
                    fileEntity.Description = dto.Description;
                }

                // Тип файла (например, с "Document" на "ProfileAvatar")
                fileEntity.Type = dto.Type;

                // Устанавливаем метку времени обновления
                fileEntity.UpdatedAt = DateTimeOffset.UtcNow;

                // 3. Сохранить изменения в БД
                await context.SaveChangesAsync(token);

                _logger.LogInformation("Метаданные файла {FileId} успешно обновлены пользователем {UserId}.", dto.Id, userId);

                // 4. Вернуть обновленный DTO (можно использовать fileEntity, но маппинг безопаснее)
                var updatedDto = _mapper.Map<FileDto>(fileEntity);

                return Result<FileDto, string>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении метаданных файла {FileId} для пользователя {UserId}.", dto.Id, userId);
                return Result<FileDto, string>.Failure($"An error occurred during file metadata update: {ex.Message}");
            }
        }

        /// <summary>
        /// Возвращает список всех файлов указанного типа для конкретного пользователя.
        /// </summary>
        public async Task<Result<IEnumerable<FileDto>, string>> GetFilesByUserIdAndTypeAsync(Guid userId, FileType fileType, CancellationToken token = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(token);

            try
            {
                // Фильтруем по UserId, исключаем удаленные и добавляем фильтр по fileType
                var userFiles = await context.Files
                    .Where(f => f.UserId == userId)
                    .Where(f => !f.IsDeleted)
                    .Where(f => f.Type == fileType) // <-- Новый фильтр по типу
                    .ToListAsync(token);

                var dtos = _mapper.Map<IEnumerable<FileDto>>(userFiles);

                return Result<IEnumerable<FileDto>, string>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении файлов типа {FileType} для пользователя {UserId}.", fileType, userId);
                return Result<IEnumerable<FileDto>, string>.Failure($"Error retrieving files of type {fileType}: {ex.Message}");
            }
        }

        public string GetUploadDirectoryName() => _uploadBasePath;
    }
}