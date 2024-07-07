using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.ServiceModels;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class PlatformService(SqlServerContext ctx)
    {
        public async Task<Platform[]> GetPlatforms(bool includeRoms)
        {
            var query = ctx.Platforms.AsNoTracking();
            if (includeRoms)
            {
                query = query.Include(m => m.RomFiles);
            }
            return await query.ToArrayAsync();
        }

        public async Task<Platform?> GetPlatform(int platform, bool includeRoms)
        {
            var query = ctx.Platforms.AsNoTracking();
            if (includeRoms)
            {
                query = query.Include(m => m.RomFiles);
            }
            return await query.FirstOrDefaultAsync(m => m.Id == platform);
        }

        public async Task<int> GetRomCount(int platform)
        {
            return await ctx.RomFiles.CountAsync(m => m.PlatformId == platform);
        }

        public async Task<PlatformCountModel[]> GetAllRomCount()
        {
            var counts = await ctx.RomFiles
                .GroupBy(m => m.PlatformId)
                .Select(m => new { Platform = m.Key, RomCount = m.Count() })
                .ToArrayAsync();
            return counts
                .Select(m => new PlatformCountModel(m.Platform, m.RomCount))
                .ToArray();
        }
    }
}
