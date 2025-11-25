using CandidateSearchSystem.Contracts.Interface;
using CandidateSearchSystem.Contracts.Service;
using CandidateSearchSystem.Data;
using CandidateSearchSystem.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CandidateSearchSystem.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCandidateSearchSystem(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            ConfigureDatabase(services, configuration);
            ConfigureIdentity(services, configuration);
            ConfigureAuthentication(services, configuration);
            ConfigureSession(services);
            ConfigureOptions(services, configuration);
            ConfigureCoreServices(services, configuration);

            if (env.IsDevelopment())
                ConfigureMockServices(services, configuration);

            return services;
        }

        private static void ConfigureMockServices(IServiceCollection services, IConfiguration configuration)
        {
            // Добавление мок-сервисов для разработки и тестирования
        }
        private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileUploadSettings>(configuration.GetSection("FileUpload"));
        }
        private static void ConfigureCoreServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(_ => { }, typeof(AppMappingProfile));
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IFileService, FileUploadService>();
        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        private static void ConfigureIdentity(IServiceCollection services, IConfiguration configuration)
        {
            var identitySection = configuration.GetSection("Identity:Password");

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = identitySection.GetValue("RequireDigit", false);
                options.Password.RequireLowercase = identitySection.GetValue("RequireLowercase", false);
                options.Password.RequireUppercase = identitySection.GetValue("RequireUppercase", false);
                options.Password.RequireNonAlphanumeric = identitySection.GetValue("RequireNonAlphanumeric", false);
                options.Password.RequiredLength = identitySection.GetValue("RequiredLength", 6);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        }
        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var cookieSection = configuration.GetSection("Authentication:Cookie");
            var expireMinutes = cookieSection.GetValue<int?>("ExpireMinutes") ?? 30;
            var sliding = cookieSection.GetValue<bool?>("SlidingExpiration") ?? false;

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = cookieSection.GetValue("LoginPath", "/account/authorization");
                options.AccessDeniedPath = cookieSection.GetValue("AccessDeniedPath", "/account/access-denied");
                options.ExpireTimeSpan = TimeSpan.FromMinutes(expireMinutes);
                options.SlidingExpiration = sliding;
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "application_token";
            });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole(ApplicationRole.Admin));
        }
        private static void ConfigureSession(IServiceCollection services)
        {
            // 1. Добавление распределенного кэша в памяти (для хранения данных сессии)
            services.AddDistributedMemoryCache();

            // 2. Добавление сервиса сессии
            services.AddSession(options =>
            {
                // Настройте время ожидания. Рекомендуется использовать то же время, что и для куки аутентификации
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Куки сессии необходим для работы
            });
        }
    }
}
