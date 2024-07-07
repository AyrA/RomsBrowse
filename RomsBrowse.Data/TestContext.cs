using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RomsBrowse.Data
{
    [AutoDIRegister(nameof(Register))]
    public class TestContext(DbContextOptions<TestContext> opt) : DbContext(opt)
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
                throw new InvalidOperationException("Connection string has not been set");
            }
            dbOpt.UseSqlServer(ConnectionString, sqlOpt =>
            {
                sqlOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                sqlOpt.MigrationsAssembly(typeof(SqlServerContext).Assembly.GetName().Name);
                sqlOpt.EnableRetryOnFailure(2);
                sqlOpt.CommandTimeout(5);
            });
            dbOpt.EnableSensitiveDataLogging(true);
            dbOpt.EnableDetailedErrors(true);
            base.OnConfiguring(dbOpt);
        }

        public static void Register(IServiceCollection services)
        {
            services.AddDbContext<TestContext>();
        }
    }
}
