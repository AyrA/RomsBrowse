using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data.Conversions;
using RomsBrowse.Data.Models;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Data;

public abstract class ApplicationContext(DbContextOptions opt, DbContextSettingsProvider settings) : DbContext(opt)
{
    protected readonly DbContextSettingsProvider settings = settings;

    public bool IsConfigured { get; protected set; }

    public DbSet<Platform> Platforms { get; set; }

    public DbSet<RomFile> RomFiles { get; set; }

    public DbSet<SaveData> SaveData { get; set; }

    public DbSet<Setting> Settings { get; set; }

    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Resets table indexes back to the initial value
    /// </summary>
    /// <typeparam name="T">Table model</typeparam>
    /// <returns>True if successfully reset</returns>
    /// <remarks>This should never be called if table <typeparamref name="T"/> is not empty</remarks>
    public abstract bool ResetIndex<T>();

    /// <summary>
    /// Searches for ROMs using a database specific mechanism
    /// </summary>
    /// <param name="text">Text to search</param>
    /// <returns>Query with search filter applied</returns>
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
