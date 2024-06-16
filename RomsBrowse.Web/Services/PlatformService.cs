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
    }
}
