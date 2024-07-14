using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RomsBrowse.Data.Models;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Data
{
    public class SqlServerContext : ApplicationContext
    {
        private readonly DbContextSettingsProvider _settings;
        private readonly MemoryCacheProvider _memCache;
        private readonly ILoggerFactory _loggerFactory;

        public SqlServerContext(DbContextOptions<SqlServerContext> opt, DbContextSettingsProvider settings, MemoryCacheProvider memCache, ILoggerFactory loggerFactory) : base(opt, settings)
        {
            IsConfigured = settings.IsConnectionStringSet;
            _settings = settings;
            _memCache = memCache;
            _loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder dbOpt)
        {
            if (!dbOpt.IsConfigured)
            {
                if (_settings.IsConnectionStringSet)
                {
                    dbOpt.UseSqlServer(_settings.GetSettings().ConnectionString, sqlOpt =>
                    {
                        sqlOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        sqlOpt.MigrationsAssembly(typeof(SqlServerContext).Assembly.GetName().Name);
                        sqlOpt.EnableRetryOnFailure();
                        sqlOpt.CommandTimeout(10);
                    });
                    //Note: This caches queries, and not data
                    dbOpt.UseMemoryCache(_memCache.Cache);
                    dbOpt.UseLoggerFactory(_loggerFactory);
#if DEBUG
                    dbOpt.EnableSensitiveDataLogging(true);
                    dbOpt.EnableDetailedErrors(true);
#endif
                }
            }
            base.OnConfiguring(dbOpt);
        }

        public static void Register(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext, SqlServerContext>();
        }

        public override bool ResetIndex<T>()
        {
            var entityType = Model.FindEntityType(typeof(T))
                ?? throw new ArgumentException($"Type {typeof(T)} has no entity");

            var tableName = entityType.GetSchemaQualifiedTableName()
                ?? throw new ArgumentException($"Type {typeof(T)} is not mapped to a table");

            try
            {
                Database.ExecuteSqlRaw("DBCC CHECKIDENT ({0}, RESEED, 0);", tableName);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override IQueryable<RomFile> SearchRoms(string text)
        {
            return RomFiles.Where(m => EF.Functions.FreeText(m.DisplayName, text));
        }
    }
}
