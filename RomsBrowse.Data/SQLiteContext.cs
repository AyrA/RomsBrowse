using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RomsBrowse.Data.Models;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Data
{
    public class SQLiteContext : ApplicationContext
    {
        private readonly MemoryCacheProvider _memCache;
        private readonly ILoggerFactory _loggerFactory;

        public SQLiteContext(DbContextOptions<SQLiteContext> opt, DbContextSettingsProvider settings, MemoryCacheProvider memCache, ILoggerFactory loggerFactory) : base(opt, settings)
        {
            IsConfigured = settings.IsConnectionStringSet;
            _memCache = memCache;
            _loggerFactory = loggerFactory;
        }

        public override bool ResetIndex<T>()
        {
            var entityType = Model.FindEntityType(typeof(T))
                ?? throw new ArgumentException($"Type {typeof(T)} has no entity");

            var tableName = entityType.GetSchemaQualifiedTableName()
                ?? throw new ArgumentException($"Type {typeof(T)} is not mapped to a table");

            return 0 < Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = {0}", tableName);
        }

        public override IQueryable<RomFile> SearchRoms(string text)
        {
            return RomFiles.Where(m => EF.Functions.Like(m.DisplayName, $"%{text}%"));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder dbOpt)
        {
            if (!dbOpt.IsConfigured)
            {
                dbOpt.UseSqlite(settings.GetSettings().ConnectionString, sqlOpt =>
                {
                    sqlOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                    sqlOpt.MigrationsAssembly(typeof(SQLiteContext).Assembly.GetName().Name);
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
            base.OnConfiguring(dbOpt);
        }
    }
}
