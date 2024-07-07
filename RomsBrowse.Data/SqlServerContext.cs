using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Data
{
    [AutoDIRegister(nameof(Register))]
    public class SqlServerContext : BaseContext
    {
        private readonly DbContextSettingsProvider _settings;
        private readonly MemoryCacheProvider _memCache;

        public SqlServerContext(DbContextOptions<SqlServerContext> opt, DbContextSettingsProvider settings, MemoryCacheProvider memCache) : base(opt)
        {
            IsConfigured = settings.IsConnectionStringSet;
            _settings = settings;
            _memCache = memCache;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder dbOpt)
        {
            if (!dbOpt.IsConfigured)
            {
                if (_settings.IsConnectionStringSet)
                {
                    dbOpt.UseSqlServer(_settings.GetConnectionString(), sqlOpt =>
                    {
                        sqlOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        sqlOpt.MigrationsAssembly(typeof(SqlServerContext).Assembly.GetName().Name);
                        sqlOpt.EnableRetryOnFailure();
                        sqlOpt.CommandTimeout(10);
                    });
                    //Note: This caches queries, and not data
                    dbOpt.UseMemoryCache(_memCache.Cache);
#if DEBUG
                    dbOpt.EnableSensitiveDataLogging(true);
                    dbOpt.EnableDetailedErrors(true);
#endif
                }
            }
            else
            {
                base.OnConfiguring(dbOpt);
            }
        }

        public static void Register(IServiceCollection services)
        {
            services.AddDbContext<BaseContext, SqlServerContext>();
        }

        public bool ResetIndex<T>()
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
    }
}
