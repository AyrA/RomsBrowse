using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class UserService(RomsContext ctx, ILogger<UserService> logger)
    {
        public int Cleanup(TimeSpan maxAge)
        {
            logger.LogInformation("Running User cleanup. Removing entries older than {Cutoff}", maxAge);
            var cutoff = DateTime.UtcNow.Subtract(maxAge);
            return
                ctx.SaveStates.Where(m => m.User.LastLogin < cutoff).ExecuteDelete() +
                ctx.Users.Where(m => m.LastLogin < cutoff).ExecuteDelete();
        }
    }
}
