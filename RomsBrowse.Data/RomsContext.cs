﻿using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Data
{
    [AutoDIRegister(nameof(Register))]
    public class RomsContext(DbContextOptions<RomsContext> opt) : DbContext(opt)
    {
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<RomFile> RomFiles { get; set; }

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
    }
}