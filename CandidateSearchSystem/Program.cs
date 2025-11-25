using CandidateSearchSystem.Components;
using CandidateSearchSystem.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace CandidateSearchSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Настройка DataProtection внутри проекта
            var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
                .SetApplicationName("CandidateSearchSystem"); 

            builder.Services.AddCandidateSearchSystem(builder.Configuration, builder.Environment);
            builder.Services.AddControllers();
            builder.Services.AddCascadingAuthenticationState();
            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            await app.UseCandidateSearchSystemAsync();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();

            app.UseAntiforgery();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
