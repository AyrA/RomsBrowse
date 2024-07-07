using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RomsBrowse.Data.Services
{
    [AutoDIRegister(nameof(Register))]
    internal class ServiceRegistration
    {
        public static void Register(IServiceCollection services)
        {
            services.AddScoped<ApplicationContext>(GetContext);
        }

        private static ApplicationContext GetContext(IServiceProvider provider)
        {
            var ctxService = provider.GetRequiredService<DbContextSettingsProvider>();
            if (ctxService.IsConnectionStringSet)
            {
                var settings = ctxService.GetSettings();
                var memcache = provider.GetRequiredService<MemoryCacheProvider>();
                return settings.DbProvider switch
                {
                    "mssql" => new SqlServerContext(new DbContextOptions<SqlServerContext>(), ctxService, memcache),
                    "sqlite" => new SQLiteContext(new DbContextOptions<SQLiteContext>(), ctxService, memcache),
                    _ => throw new NotImplementedException($"Unknown db type: {settings.DbProvider}"),
                };
            }
            throw new NotImplementedException();
        }
    }
}
