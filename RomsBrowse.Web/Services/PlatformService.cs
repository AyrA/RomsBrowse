using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class PlatformService(RomsContext ctx)
    {
        public async Task<Platform[]> GetPlatforms(bool includeRoms)
        {
            var query = ctx.Platforms.AsNoTracking();
            if (includeRoms)
            {
                query = query.Include(m => m.Roms);
            }
            return await query.ToArrayAsync();
        }

        public async Task<Platform?> GetPlatform(int platform, bool includeRoms)
        {
            var query = ctx.Platforms.AsNoTracking();
            if (includeRoms)
            {
                query = query.Include(m => m.Roms);
            }
            return await query.FirstOrDefaultAsync(m => m.Id == platform);
        }

        public async Task<int> GetRomCount(int platform)
        {
            return await ctx.RomFiles.CountAsync(m => m.PlatformId == platform);
        }
    }
}
