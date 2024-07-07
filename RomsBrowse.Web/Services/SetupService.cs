using AyrA.AutoDI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class SetupService(DbContextSettingsProvider cstr, IServiceProvider provider, ILogger<SetupService> logger)
    {
        public bool IsConfigured => cstr.IsConnectionStringSet;

        public string DataDirectory => cstr.DataDirectry;

        public IActionResult SetupRedirect => new RedirectToActionResult("Index", "Init", null);

        public void SetConnectionString(string connStr, string dbProvider)
        {
            cstr.SetSettings(connStr, dbProvider);
        }

        public async Task FullInit()
        {
            await RunMigrations();
            await InitData();
        }

        public async Task RunMigrations()
        {
            using var scope = provider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
            using var logScope = logger.BeginScope(ctx);
            if (pending.Count > 0)
            {
                logger.LogInformation("Applying all pending migrations:");
                foreach (var mig in pending)
                {
                    logger.LogInformation("Pending: {Migration}", mig);
                }
                try
                {
                    await ctx.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Failed to apply migrations");
                    throw;
                }
                logger.LogInformation("Done!");
            }
            else
            {
                logger.LogInformation("Database has no pending changes to be applied");
            }
        }

        public async Task InitData()
        {
            using var scope = provider.CreateScope();

            //Fill menu and platform fields to be used in views
            var menuService = scope.ServiceProvider.GetRequiredService<MainMenuService>();
            var platformService = scope.ServiceProvider.GetRequiredService<PlatformService>();
            var counts = await platformService.GetAllRomCount();
            menuService.SetMenuItems(await platformService.GetPlatforms(false), counts);

            //Create initial settings if needed
            var user = scope.ServiceProvider.GetRequiredService<UserService>();
            var ss = scope.ServiceProvider.GetRequiredService<SettingsService>();

            //If no admin exists, change admin token on every start
            //Delete token if admin user exists
            if (!await user.HasAdmin())
            {
                ss[SettingsService.KnownSettings.AdminToken] = Guid.NewGuid()
                    .ToString()
                    .ToUpperInvariant();
            }
            else
            {
                ss.Delete(SettingsService.KnownSettings.AdminToken);
            }
            ss.AddDefault(SettingsService.KnownSettings.AllowRegister, false);
            ss.AddDefault(SettingsService.KnownSettings.AnonymousPlay, false);
            ss.AddDefault(SettingsService.KnownSettings.MaxSaveStatesPerUser, 10);
            ss.AddDefault(SettingsService.KnownSettings.SaveStateExpiration, TimeSpan.FromDays(30));
            ss.AddDefault(SettingsService.KnownSettings.UserExpiration, TimeSpan.FromDays(30));
        }
    }
}
