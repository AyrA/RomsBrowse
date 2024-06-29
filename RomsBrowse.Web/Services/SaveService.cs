using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common.Services;
using RomsBrowse.Data;
using RomsBrowse.Web.ServiceModels;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class SaveService(ICompressionService compressor, RomsContext ctx, SettingsService ss, ILogger<SaveService> logger)
    {
        public async Task SaveState(string username, int gameId, byte[] imageData, byte[] stateData)
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
            state.Data = compressor.Compress(stateData);
            state.Image = imageData;
            state.Validate();
            user.LastActivity = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public Task SaveState(string username, int gameId, Stream imageData, Stream stateData)
        {
            using var ms1 = new MemoryStream();
            using var ms2 = new MemoryStream();
            imageData.CopyTo(ms1);
            stateData.CopyTo(ms2);
            return SaveState(username, gameId, ms1.ToArray(), ms2.ToArray());
        }

        public async Task<LoadStateModel?> GetState(int gameId, string username)
        {
            if (gameId < 1)
            {
                return null;
            }

            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username);
            if (user == null)
            {
                return null;
            }
            var state = await ctx.SaveStates.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == user.Id && m.RomFileId == gameId);
            if (state == null)
            {
                return null;
            }
            return new(state.Image, compressor.Decompress(state.Data));
        }

        public async Task<bool> HasState(int id, string username)
        {
            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username);
            if (user == null)
            {
                return false;
            }
            return await ctx.SaveStates.AsNoTracking().AnyAsync(m => m.RomFileId == id && m.UserId == user.Id);
        }

        public async Task SaveSRAM(string username, int gameId, byte[] sramData)
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
            var sram = await ctx.SRAMs
                .FirstOrDefaultAsync(m => m.UserId == user.Id && m.RomFileId == rom.Id);
            if (sram == null)
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
                sram = new()
                {
                    UserId = user.Id,
                    User = user,
                    RomFile = rom,
                    RomFileId = rom.Id,
                };
                ctx.SRAMs.Add(sram);
            }
            sram.Created = DateTime.UtcNow;
            sram.Data = compressor.Compress(sramData);
            sram.Validate();
            user.LastActivity = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        public Task SaveSRAM(string username, int gameId, Stream stateData)
        {
            using var ms1 = new MemoryStream();
            stateData.CopyTo(ms1);
            return SaveSRAM(username, gameId, ms1.ToArray());
        }

        public async Task<byte[]?> GetSRAM(int gameId, string username)
        {
            if (gameId < 1)
            {
                return null;
            }

            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username);
            if (user == null)
            {
                return null;
            }
            var sram = await ctx.SRAMs.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == user.Id && m.RomFileId == gameId);
            if (sram == null)
            {
                return null;
            }
            return compressor.Decompress(sram.Data);
        }

        public async Task<bool> HasSRAM(int id, string username)
        {
            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username);
            if (user == null)
            {
                return false;
            }
            return await ctx.SRAMs.AnyAsync(m => m.RomFileId == id && m.UserId == user.Id);
        }

        public async Task<SaveListViewModel> GetSaves(string username)
        {
            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username)
                ?? throw new ArgumentException("User does not exist");

            var srams = await ctx.SRAMs
                .AsNoTracking()
                .Include(m => m.RomFile)
                .ThenInclude(m => m.Platform)
                .Where(m => m.UserId == user.Id)
                .AsSplitQuery()
                .ToListAsync();

            var saves = await ctx.SaveStates
                .AsNoTracking()
                .Include(m => m.RomFile)
                .ThenInclude(m => m.Platform)
                .Where(m => m.UserId == user.Id)
                .AsSplitQuery()
                .ToListAsync();

            var vm = new SaveListViewModel();
            vm.SRAMs.AddRange(srams.Select(m => new SRAMViewModel(m, compressor)));
            vm.SaveStates.AddRange(saves.Select(m => new SaveStateViewModel(m, compressor)));
            return vm;
        }

        public int Cleanup(TimeSpan maxAge)
        {
            if (maxAge > TimeSpan.Zero)
            {
                logger.LogInformation("Running state and SRAM cleanup. Removing entries older than {Cutoff}", maxAge);
                var cutoff = DateTime.UtcNow.Subtract(maxAge);
                return
                    ctx.SaveStates.Where(m => m.Created < cutoff && !m.User.Flags.HasFlag(Data.Enums.UserFlags.NoExpireSaveState)).ExecuteDelete() +
                    ctx.SRAMs.Where(m => m.Created < cutoff && !m.User.Flags.HasFlag(Data.Enums.UserFlags.NoExpireSaveState)).ExecuteDelete();
            }
            else
            {
                logger.LogInformation("Skipping state and SRAM cleanup. States are set to not currently expire");
            }
            return 0;
        }
    }
}
