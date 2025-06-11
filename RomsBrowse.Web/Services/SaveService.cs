using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common.Services;
using RomsBrowse.Data;
using RomsBrowse.Data.Enums;
using RomsBrowse.Web.ServiceModels;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class SaveService(ICompressionService compressor, ApplicationContext ctx, SettingsService ss, ILogger<SaveService> logger)
    {
        public async Task SaveData(int gameId, string username, byte[] imageData, byte[] stateData)
        {
            await SaveData(gameId, username, imageData, stateData, SaveFlags.State);
        }

        public Task SaveData(int gameId, string username, Stream imageData, Stream stateData)
        {
            using var ms1 = new MemoryStream();
            using var ms2 = new MemoryStream();
            imageData.CopyTo(ms1);
            stateData.CopyTo(ms2);
            return SaveData(gameId, username, ms1.ToArray(), ms2.ToArray());
        }

        public async Task<LoadSaveModel?> GetState(int gameId, string username)
        {
            return await GetData(gameId, username, SaveFlags.State);
        }

        public async Task<bool> HasState(int gameId, string username)
        {
            return await HasData(gameId, username, SaveFlags.State);
        }

        public async Task SaveSRAM(int gameId, string username, byte[] imageData, byte[] sramData)
        {
            await SaveData(gameId, username, imageData, sramData, SaveFlags.SRAM);
        }

        public Task SaveSRAM(int gameId, string username, Stream imageData, Stream stateData)
        {
            using var ms1 = new MemoryStream();
            using var ms2 = new MemoryStream();
            stateData.CopyTo(ms1);
            imageData.CopyTo(ms2);
            return SaveSRAM(gameId, username, ms2.ToArray(), ms1.ToArray());
        }

        public async Task<LoadSaveModel?> GetSRAM(int gameId, string username)
        {
            return await GetData(gameId, username, SaveFlags.SRAM);
        }

        public async Task<bool> HasSRAM(int gameId, string username)
        {
            return await HasData(gameId, username, SaveFlags.SRAM);
        }

        public async Task<SaveListViewModel> GetSaves(string username)
        {
            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username)
                ?? throw new ArgumentException("User does not exist");

            var saves = await ctx.SaveData
                .AsNoTracking()
                .Include(m => m.RomFile)
                .ThenInclude(m => m.Platform)
                .Where(m => m.UserId == user.Id)
                .OrderByDescending(m => m.Created)
                .AsSplitQuery()
                .ToListAsync();

            var vm = new SaveListViewModel();
            vm.SRAMs.AddRange(saves.Where(m => m.Flags.HasFlag(SaveFlags.SRAM)).Select(m => new SaveDataViewModel(m, compressor)));
            vm.SaveStates.AddRange(saves.Where(m => m.Flags.HasFlag(SaveFlags.State)).Select(m => new SaveDataViewModel(m, compressor)));
            return vm;
        }

        public int Cleanup(TimeSpan maxAge)
        {
            if (maxAge > TimeSpan.Zero)
            {
                logger.LogInformation("Running state and SRAM cleanup. Removing entries older than {Cutoff}", maxAge);
                var cutoff = DateTime.UtcNow.Subtract(maxAge);
                return
                    ctx.SaveData.Where(m => m.Created < cutoff && !m.User.Flags.HasFlag(UserFlags.Admin) && !m.User.Flags.HasFlag(UserFlags.NoExpireSaveState)).ExecuteDelete();
            }
            else
            {
                logger.LogInformation("Skipping state and SRAM cleanup. States are set to not currently expire");
            }
            return 0;
        }

        public async Task ResetTimer(int gameId, string username, SaveFlags type)
        {
            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username)
                ?? throw new Exception("User does not exist");
            if (!Enum.IsDefined(type))
            {
                throw new ArgumentException("Invalid save type");
            }
            var changed = await ctx.SaveData
                .Where(m => m.UserId == user.Id && m.RomFileId == gameId && m.Flags.HasFlag(type))
                .ExecuteUpdateAsync(m => m.SetProperty(p => p.Created, DateTime.UtcNow));
            if (changed == 0)
            {
                throw new Exception($"No {type} data found for this game");
            }
        }

        public async Task Delete(int gameId, string username, SaveFlags type)
        {
            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username)
                ?? throw new Exception("User does not exist");
            if (!Enum.IsDefined(type))
            {
                throw new ArgumentException("Invalid save type");
            }
            var changed = await ctx.SaveData
                .Where(m => m.UserId == user.Id && m.RomFileId == gameId && m.Flags.HasFlag(type))
                .ExecuteDeleteAsync();
            if (changed == 0)
            {
                throw new Exception($"No {type} data found for this game");
            }
        }

        private async Task SaveData(int gameId, string username, byte[] imageData, byte[] stateData, SaveFlags type)
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
            var state = await ctx.SaveData
                .FirstOrDefaultAsync(m => m.UserId == user.Id && m.RomFileId == rom.Id && m.Flags.HasFlag(type));
            if (state == null)
            {
                var maxStates = ss.GetValue<int>(SettingsService.KnownSettings.MaxSaveStatesPerUser);
                if (maxStates > 0)
                {
                    //Keep last "n" states, and delete the rest
                    await ctx.SaveData
                        .Where(m => m.UserId == user.Id && m.Flags.HasFlag(type))
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
                    Flags = type
                };
                ctx.SaveData.Add(state);
            }
            state.Created = DateTime.UtcNow;
            state.Data = compressor.Compress(stateData);
            state.Image = imageData;
            state.Validate();
            user.LastActivity = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        private async Task<LoadSaveModel?> GetData(int gameId, string username, SaveFlags type)
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
            var state = await ctx.SaveData
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == user.Id && m.RomFileId == gameId && m.Flags.HasFlag(type));
            if (state == null)
            {
                return null;
            }
            return new(state.Image, compressor.Decompress(state.Data));
        }

        private async Task<bool> HasData(int gameId, string username, SaveFlags type)
        {
            var user = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(m => m.Username == username);
            if (user == null)
            {
                return false;
            }
            return await ctx.SaveData
                .AnyAsync(m => m.RomFileId == gameId && m.UserId == user.Id && m.Flags.HasFlag(type));

        }
    }
}
