using CandidateSearchSystem.Contracts.Interface;
using CandidateSearchSystem.Data;
using CandidateSearchSystem.Data.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace CandidateSearchSystem.Extensions
{
    public static class DatabaseInitializerExtensions
    {
        // 1. Статический экземпляр Random для корректной рандомизации
        private static readonly Random Rnd = new();

        // Main orchestration method
        public static async Task MigrateAndSeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // Clear Data Protection Keys to invalidate cookies after DB reset
                //await ClearDataProtectionKeys(services);

                // 1. Static/Essential data setup (Migrations, Roles)
                await EnsureStaticDataAsync(services);

                // 2. Test-specific data setup (Test Users)
                if (app.Environment.IsDevelopment())
                    await EnsureTestDataAsync(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ошибка при инициализации базы данных");
                throw;
            }
        }

        // Handles essential, non-test data: Migrations and Roles
        private static async Task EnsureStaticDataAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
            await EnsureRoleDataAsync(services);
        }
        private static async Task EnsureRoleDataAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            foreach (var role in ApplicationRole.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole(role));
                }
            }
        }
        private static async Task ClearDataProtectionKeys(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            var context = services.GetRequiredService<ApplicationDbContext>();
            var fileuploader = services.GetRequiredService<IFileService>();

            // Удаляем базу данных
            await context.Database.EnsureDeletedAsync();
            logger.LogInformation("Database deleted successfully.");

            // Получаем IKeyManager для работы с ключами Data Protection
            var keyManager = services.GetService<IKeyManager>();
            if (keyManager == null)
            {
                logger.LogWarning("IKeyManager service not found. Cannot clear Data Protection Keys.");
            }
            else
            {
                try
                {
                    // Удаляем папку с ключами
                    string keysFolder = Path.Combine(Directory.GetCurrentDirectory(), "DataProtectionKeys");
                    if (Directory.Exists(keysFolder))
                    {
                        Directory.Delete(keysFolder, recursive: true);
                        logger.LogInformation("Deleted Data Protection Keys folder: All cookies are invalidated.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to clear/revoke Data Protection Keys.");
                }
            }

            // Удаляем каталог upload из www
            try
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileuploader.GetUploadDirectoryName());
                if (Directory.Exists(uploadFolder))
                {
                    Directory.Delete(uploadFolder, recursive: true);
                    logger.LogInformation("Deleted www/upload folder: All uploaded files are removed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete www/upload folder.");
            }
        }



        #region [Handles test-specific data: Test News, Profiles, Tests]
        private static async Task EnsureTestDataAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Если в базе уже есть много данных, можно пропустить инициализацию
            if (context.CandidateProfiles.Any())
            {
                return;
            }

            // 2. Создание Новостей
            await EnsureTestNewsAsync(context);

            await EnsureTestUserAsync(context, userManager);

            // Сохраняем все добавленные сущности
            await context.SaveChangesAsync();
        }
        private static async Task EnsureTestUserAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // 2. Вспомогательная функция для генерации случайной даты рождения
            DateTime GetRandomDateOfBirth()
            {
                // Диапазон: от 1980-01-01 до 2005-12-31
                DateTime startDate = new(1980, 1, 1);
                DateTime endDate = new(2005, 12, 31);

                int range = (endDate - startDate).Days;
                return startDate.AddDays(Rnd.Next(range + 1));
            }

            async Task EnsureUser(string email, string role, string firstName, string lastName, string? patronymic = null)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Patronymic = patronymic,
                        CreatedAt = DateTime.UtcNow,
                        DateOfBirth = GetRandomDateOfBirth()
                    };

                    // NOTE: Используйте здесь пароль, который гарантированно пройдет ваши политики безопасности.
                    // В данном случае, используем email в качестве пароля.
                    var result = await userManager.CreateAsync(user, email);

                    if (result.Succeeded)
                    {
                        // 4. Убедитесь, что роль существует, прежде чем добавлять
                        await userManager.AddToRoleAsync(user, role);

                        // 🔥 ИСПРАВЛЕНИЕ: ПРОВЕРКА НА СУЩЕСТВОВАНИЕ КОНТАКТА
                        var existingContact = await context.Contacts
                            .FirstOrDefaultAsync(c =>
                                c.UserId == user.Id &&
                                c.Type == Data.Constants.ContactType.Email &&
                                c.Value == email);

                        if (existingContact == null)
                        {
                            // Контакта нет, можно создавать
                            var contact = new Contact
                            {
                                UserId = user.Id,
                                Type = Data.Constants.ContactType.Email,
                                Value = email,
                                IsPrimary = true
                            };
                            context.Add(contact);
                        }
                    }
                }
            }
            
            // --- Исправленные вызовы тестовых пользователей ---
            // 5. Передаем корректные параметры: email, СТРОКОВОЕ имя роли, Имя, Фамилия
            await EnsureUser("admin@test.com", ApplicationRole.Admin, "Алексей", "Админов");
            await EnsureUser("candidate@test.com", ApplicationRole.Candidate, "Иван", "Кандидатов", "Петрович");
            await EnsureUser("recruiter@test.com", ApplicationRole.Recruiter, "Елена", "Рекрутова");
            await EnsureUser("user@test.com", ApplicationRole.Candidate, "Мария", "Тестова");

        }

        private static async Task EnsureTestNewsAsync(ApplicationDbContext context)
        {
            // Использование данных из MockNewsService.cs в качестве статических
            var newsData = new List<NewsPost>
            {
                new() {
                    Id = Guid.NewGuid(),
                    CreatedAt = new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc),
                    Title = "⚡️ Анонс: Новый профессиональный тест по C#",
                    Text = "Мы рады объявить о выходе нового продвинутого теста...",
                    Author = "Администратор",
                    Level =  Data.Constants.NewsLevel.Release
                },
                new() {
                    Id = Guid.NewGuid(),
                    CreatedAt = new DateTime(2025, 11, 28, 15, 30, 0, DateTimeKind.Utc),
                    Title = "⚠️ Срочный HotFix: Устранена критическая ошибка 500",
                    Text = "Проблема была обнаружена и оперативно исправлена...",
                    Author = "Администратор",
                    Level = Data.Constants.NewsLevel.HotFix
                },
                // ... другие новости ...
            };

            var existingTitles = await context.NewsPosts
                 .Select(n => n.Title)
                 .ToListAsync();

            var newNews = newsData
                .Where(n => !existingTitles.Contains(n.Title))
                .ToList();

            if (newNews.Count != 0)
                await context.NewsPosts.AddRangeAsync(newNews);
        }


        #endregion // End of Test Data Region
    }


}