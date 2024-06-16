using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class RomSearchService(RomsContext ctx)
    {
        public async Task<RomFile[]> Search(string terms)
        {
            return await ctx.RomFiles
                .Where(m => EF.Functions.FreeText(m.DisplayName, terms))
                .ToArrayAsync();
        }
        public async Task<RomFile[]> Search(string terms, int platformId)
        {
            return await ctx.RomFiles
                .Where(m => m.PlatformId == platformId && EF.Functions.FreeText(m.DisplayName, terms))
                .ToArrayAsync();
        }
    }
}
