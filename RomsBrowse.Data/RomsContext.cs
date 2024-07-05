using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RomsBrowse.Data.Conversions;
using RomsBrowse.Data.Models;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Data
{
    [AutoDIRegister(nameof(Register))]
    public class RomsContext(DbContextOptions<RomsContext> opt, ConnectionStringProvider connStr) : DbContext(opt)
    {
        public bool IsConfigured => connStr.IsSet;

        public DbSet<Platform> Platforms { get; set; }

        public DbSet<RomFile> RomFiles { get; set; }

        public DbSet<SaveData> SaveData { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Setting> Settings { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            ArgumentNullException.ThrowIfNull(configurationBuilder);

            configurationBuilder.Properties<DateTime>()
                .HaveConversion<DateTimeAsUtcValueConverter>();
            configurationBuilder.Properties<DateTime?>()
                .HaveConversion<NullableDateTimeAsUtcValueConverter>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder dbOpt)
        {
            if (!dbOpt.IsConfigured)
            {
                if (connStr.IsSet)
                {
                    dbOpt.UseSqlServer(connStr.GetConnectionString(), sqlOpt =>
                    {
                        sqlOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        sqlOpt.MigrationsAssembly(typeof(RomsContext).Assembly.GetName().Name);
                        sqlOpt.EnableRetryOnFailure();
                        sqlOpt.CommandTimeout(10);
                    });
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
            services.AddDbContext<RomsContext>();
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
