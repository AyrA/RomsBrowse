using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class SaveStateService(RomsContext ctx)
    {
        public int Cleanup(TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow.Subtract(maxAge);
            return ctx.SaveStates.Where(m => m.Created < cutoff).ExecuteDelete();
        }
    }
}
