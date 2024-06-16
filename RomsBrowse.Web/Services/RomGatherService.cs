using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.ServiceModels;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class RomGatherService(IConfiguration config, RomsContext ctx)
    {
        private readonly string rootDir = config.GetValue<string>("RomDir")
            ?? throw new InvalidOperationException("'RomDir' not set");

        private RomDirConfig[] GetConfig()
        {
            return Path
                .Combine(rootDir, "config.json")
                .FromJsonRequired<RomDirConfig[]>();
        }

        public async Task GatherRoms()
        {
            var config = GetConfig();
            var platforms = await ctx.Platforms
                .Include(m => m.Roms)
                .AsNoTracking()
                .ToListAsync();
            throw new NotImplementedException();
        }
    }
}
