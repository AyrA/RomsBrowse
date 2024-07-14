using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data.Conversions;
using RomsBrowse.Data.Models;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Data
{
    public abstract class ApplicationContext(DbContextOptions opt, DbContextSettingsProvider settings) : DbContext(opt)
    {
        protected readonly DbContextSettingsProvider settings = settings;

        public bool IsConfigured { get; protected set; }

        public DbSet<Platform> Platforms { get; set; }

        public DbSet<RomFile> RomFiles { get; set; }

        public DbSet<SaveData> SaveData { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<User> Users { get; set; }

        public abstract bool ResetIndex<T>();

        public abstract IQueryable<RomFile> SearchRoms(string text);

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            ArgumentNullException.ThrowIfNull(configurationBuilder);

            configurationBuilder.Properties<DateTime>()
                .HaveConversion<DateTimeAsUtcValueConverter>();
            configurationBuilder.Properties<DateTime?>()
                .HaveConversion<NullableDateTimeAsUtcValueConverter>();
        }
    }
}
