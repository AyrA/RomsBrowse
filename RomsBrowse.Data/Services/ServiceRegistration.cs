using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RomsBrowse.Data.Services
{
    [AutoDIRegister(nameof(Register))]
    internal class ServiceRegistration
    {
        public static void Register(IServiceCollection services)
        {
            services.AddScoped(GetContext);
            if (EF.IsDesignTime)
            {
                var configType = services.FirstOrDefault(m => m.ServiceType == typeof(IConfiguration))
                ?? throw new InvalidOperationException("Cannot register Database context without IConfiguration");
                IConfiguration config = (IConfiguration?)configType.ImplementationInstance
                    ?? (IConfiguration?)configType.ImplementationFactory?.Invoke(null!)
                    ?? throw new InvalidOperationException("IConfiguration instance is not set");

                //Add hardcoded services that are "good enough" for design time purposes
                services.AddDbContext<SqlServerContext>(opt => opt.UseSqlServer(config.GetConnectionString(nameof(SqlServerContext))));
                services.AddDbContext<SQLiteContext>(opt => opt.UseSqlite(config.GetConnectionString(nameof(SQLiteContext))));
            }
        }

        private static ApplicationContext GetContext(IServiceProvider provider)
        {
            var ctxService = provider.GetRequiredService<DbContextSettingsProvider>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            if (ctxService.IsConnectionStringSet)
            {
                var settings = ctxService.GetSettings();
                var memcache = provider.GetRequiredService<MemoryCacheProvider>();
                return settings.DbProvider switch
                {
                    "mssql" => new SqlServerContext(new DbContextOptions<SqlServerContext>(), ctxService, memcache, loggerFactory),
                    "sqlite" => new SQLiteContext(new DbContextOptions<SQLiteContext>(), ctxService, memcache, loggerFactory),
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
