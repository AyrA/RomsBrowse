using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data.Conversions;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Data
{
    public abstract class BaseContext(DbContextOptions opt) : DbContext(opt)
    {
        public bool IsConfigured { get; protected set; }

        public DbSet<Platform> Platforms { get; set; }

        public DbSet<RomFile> RomFiles { get; set; }

        public DbSet<SaveData> SaveData { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<User> Users { get; set; }

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
