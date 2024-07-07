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
            services.AddScoped(GetContext);
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
            //Return blank context if not settings have been made.
            //This will crash services that are not aware of it,
            //But the user is locked to the init controller, which is aware.
            return new SqlServerTestContext(new DbContextOptions<SqlServerTestContext>(), ctxService);
        }
    }
}
