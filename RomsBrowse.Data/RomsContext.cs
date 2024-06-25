using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RomsBrowse.Data.Conversions;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Data
{
    [AutoDIRegister(nameof(Register))]
    public class RomsContext(DbContextOptions<RomsContext> opt) : DbContext(opt)
    {
        public DbSet<Platform> Platforms { get; set; }

        public DbSet<RomFile> RomFiles { get; set; }

        public DbSet<SaveState> SaveStates { get; set; }

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

        public static void Register(IServiceCollection services)
        {
            var configType = services.FirstOrDefault(m => m.ServiceType == typeof(IConfiguration))
                ?? throw new InvalidOperationException("Cannot register Database context without IConfiguration");
            IConfiguration config = (IConfiguration?)configType.ImplementationInstance
                ?? (IConfiguration?)configType.ImplementationFactory?.Invoke(null!)
                ?? throw new InvalidOperationException("IConfiguration instance is not set");
            services.AddDbContext<RomsContext>(dbOpt =>
            {
                dbOpt.UseSqlServer(config.GetConnectionString("Default"), sqlOpt =>
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
            });
        }

        public void ResetIndex<T>()
        {
            var entityType = Model.FindEntityType(typeof(T))
                ?? throw new ArgumentException($"Type {typeof(T)} has no entity");

            var tableName = entityType.GetSchemaQualifiedTableName()
                ?? throw new ArgumentException($"Type {typeof(T)} is not mapped to a table");

            Database.ExecuteSqlRaw("DBCC CHECKIDENT ({0}, RESEED, 0);", tableName);
        }
    }
}
