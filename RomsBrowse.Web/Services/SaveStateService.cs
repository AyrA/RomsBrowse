using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class SaveStateService(RomsContext ctx, SettingsService ss, ILogger<SaveStateService> logger)
    {
        public async Task Save(string username, int gameId, byte[] imageData, byte[] stateData)
        {
            var user = await ctx.Users.FirstOrDefaultAsync(m => m.Username == username);
            if (user == null)
            {
                return;
            }

            var rom = await ctx.RomFiles.FirstOrDefaultAsync(m => m.Id == gameId);
            if (rom == null)
            {
                return;
            }
            var state = await ctx.SaveStates
                .FirstOrDefaultAsync(m => m.UserId == user.Id && m.RomFileId == rom.Id);
            if (state == null)
            {
                var maxStates = ss.GetValue<int>(SettingsService.KnownSettings.MaxSaveStatesPerUser);
                if (maxStates > 0)
                {
                    //Keep last "n" states, and delete the rest
                    await ctx.SaveStates
                        .Where(m => m.UserId == user.Id)
                        .OrderByDescending(m => m.Created)
                        .Skip(maxStates)
                        .ExecuteDeleteAsync();
                }
                state = new()
                {
                    UserId = user.Id,
                    User = user,
                    RomFile = rom,
                    RomFileId = rom.Id,
                };
                ctx.SaveStates.Add(state);
            }
            state.Created = DateTime.UtcNow;
            state.Data = stateData;
            state.Image = imageData;
            state.Validate();
            user.LastActivity = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public Task Save(string username, int gameId, Stream imageData, Stream stateData)
        {
            using var ms1 = new MemoryStream();
            using var ms2 = new MemoryStream();
            imageData.CopyTo(ms1);
            stateData.CopyTo(ms2);
            return Save(username, gameId, ms1.ToArray(), ms2.ToArray());
        }

        public int Cleanup(TimeSpan maxAge)
        {
            if (maxAge > TimeSpan.Zero)
            {
                logger.LogInformation("Running SaveState cleanup. Removing entries older than {Cutoff}", maxAge);
                var cutoff = DateTime.UtcNow.Subtract(maxAge);
                return ctx.SaveStates.Where(m => m.Created < cutoff && !m.User.Flags.HasFlag(Data.Enums.UserFlags.NoExpireSaveState)).ExecuteDelete();
            }
            else
            {
                logger.LogInformation("Skipping SaveState cleanup. States are set to not currently expire");
            }
            return 0;
        }
    }
}
