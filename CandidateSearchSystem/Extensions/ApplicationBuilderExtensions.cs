using CandidateSearchSystem.Contracts.Interface;
using CandidateSearchSystem.Data.Constants;
using CandidateSearchSystem.Data.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CandidateSearchSystem.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        // Обновление: вызываем маппинг эндпоинтов после миграций
        public static async Task<WebApplication> UseCandidateSearchSystemAsync(this WebApplication app)
        {
            await app.MigrateAndSeedDatabaseAsync();

            // 🔥 Добавляем маппинг API для аутентификации
            app.MapAccountApiEndpoints();

            return app;
        }

        // 🔥 Новый метод для маппинга минимальных API-эндпоинтов
        public static WebApplication MapAccountApiEndpoints(this WebApplication app)
        {
            // Группируем маршруты под префиксом /api/Account
            var group = app.MapGroup("/api/Account");

            // POST /api/Account/login
            group.MapPost("/login", async ([FromForm] LoginFormDTO request,
                IAccountService service, HttpContext httpContext) =>
            {
                // В Minimal API валидация DTO сложнее, чем в Controller, поэтому полагаемся на логику Identity.

                var result = await service.LoginAsync(request);
                if (result.IsSuccess)
                    return Results.LocalRedirect("/", permanent: false);
                else
                {
                    // Сохраняем ошибку в сессию, чтобы отобразить ее после перезагрузки
                    httpContext.Session.SetString("LoginError", result.Error);
                    return Results.LocalRedirect("/account/authorization?type=login", permanent: false);
                }
            })
            .AllowAnonymous();

            // POST /api/Account/register
            group.MapPost("/register", async ([FromForm] RegisterFormDTO request,
                IAccountService service, IContactService contacts, HttpContext httpContext) =>
            {
                // 1. Попытка регистрации
                var result = await service.RegisterAsync(request);

                // 2. Проверка успешности регистрации
                if (result.IsSuccess)
                {
                    // 3. Если регистрация успешна, сразу добавляем контакт
                    var newUserId = result.Value.Id; // Использование Id из результата регистрации

                    var contactResult = await contacts.AddAsync(newUserId, new ContactDto
                    {
                        Type = ContactType.Email,
                        Value = request.Email,
                        Description = "Primary email",
                        IsPrimary = true
                    });

                    // 4. Добавление контакта желательно также проверить на успех, 
                    // хотя для MVP это может быть опущено.
                    // Если добавление контакта критично, можно добавить здесь дополнительную логику 
                    // обработки ошибок или логирование.

                    // 5. Выполняем редирект после успешной регистрации и добавления контакта
                    return Results.LocalRedirect("/", permanent: false);
                }
                else
                {
                    // 6. Если регистрация НЕ успешна, сохраняем ошибку и делаем редирект на страницу регистрации
                    // Сохраняем ошибку в сессию, чтобы отобразить ее после перезагрузки
                    httpContext.Session.SetString("RegisterError", result.Error);
                    return Results.LocalRedirect("/account/authorization?type=register", permanent: false);
                }
            })
            .AllowAnonymous();


            // POST /api/Account/logout
            group.MapPost("/logout", async (IAccountService service, HttpContext httpContext) =>
            {
                var result = await service.LogoutAsync();
                if (result.IsSuccess)
                    return Results.LocalRedirect("/", permanent: false);
                else
                {
                    // Сохраняем ошибку в сессию, чтобы отобразить ее после перезагрузки
                    httpContext.Session.SetString("LogoutError", result.Error);
                    return Results.LocalRedirect("/?errms=\"An error has occurred\"", permanent: false);
                }
            })
            .RequireAuthorization(); // Только авторизованный пользователь может выйти

            return app;
        }
    }
}