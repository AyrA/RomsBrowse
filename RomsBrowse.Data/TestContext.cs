using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RomsBrowse.Data.Models;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Data
{
    [AutoDIRegister(nameof(Register))]
    public class SqlServerTestContext(DbContextOptions<SqlServerTestContext> opt, DbContextSettingsProvider settings) : ApplicationContext(opt, settings)
    {
        public string? ConnectionString { get; set; }

        public void TestConnection()
        {
            var result = Database.SqlQueryRaw<int>("SELECT 69").ToArray();
            if (result.Length != 1)
            {
                throw new Exception($"Expected one result for test query but got {result.Length}");
            }
            if (result[0] != 69)
            {
                throw new Exception($"Expected '69' as result for test query but got {result[0]}");
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder dbOpt)
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                IsConfigured = false;
                throw new InvalidOperationException("Connection string has not been set");
            }
            dbOpt.UseSqlServer(ConnectionString, sqlOpt =>
            {
                sqlOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                sqlOpt.MigrationsAssembly(typeof(SqlServerTestContext).Assembly.GetName().Name);
                sqlOpt.EnableRetryOnFailure(2);
                sqlOpt.CommandTimeout(5);
            });
            dbOpt.EnableSensitiveDataLogging(true);
            dbOpt.EnableDetailedErrors(true);
            IsConfigured = true;
            base.OnConfiguring(dbOpt);
        }

        public static void Register(IServiceCollection services)
        {
            services.AddDbContext<SqlServerTestContext>();
        }

        public override bool ResetIndex<T>() => false;

        public override IQueryable<RomFile> SearchRoms(string text)
        {
            throw new InvalidOperationException("Not available in test context");
        }
    }
}
