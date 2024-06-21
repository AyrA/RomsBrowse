using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.ServiceModels;
using System.Security.Cryptography;

namespace RomsBrowse.Web.Services
{
    [AutoDIHostedService]
    public class RomGatherService(IServiceProvider provider, IConfiguration config, ILogger<RomGatherService> logger) : IHostedService
    {
        private readonly string rootDir = Path.GetFullPath(config.GetValue<string>("RomDir")
            ?? throw new InvalidOperationException("'RomDir' not set"));

        private Thread? t = null;
        private CancellationTokenSource? cts = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (t != null)
            {
                throw new InvalidOperationException("A gathering task is already running");
            }
            t = new Thread(GatherRoms)
            {
                IsBackground = true,
            };
            cts = new();
            t.Start();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var cancelTask = cts?.CancelAsync();

            if (cancelTask != null)
            {
                await cancelTask;
            }
            t?.Join(TimeSpan.FromSeconds(10));
            cts = null;
            t = null;
        }

        private void CheckAbort()
        {
            var token = cts?.Token
                ?? throw new InvalidOperationException("Cancellation token source was not set");
            token.ThrowIfCancellationRequested();
        }

        private void UpdateMenu()
        {
            using var scope = provider.CreateScope();
            var platformService = scope.ServiceProvider.GetRequiredService<PlatformService>();
            var menuService = scope.ServiceProvider.GetRequiredService<MainMenuService>();
            menuService.SetMenuItems(platformService.GetPlatforms(false).Result, platformService.GetAllRomCount().Result);
        }

        /// <summary>
        /// Scans for new, updated, and deleted ROMS
        /// </summary>
        /// <exception cref="InvalidOperationException">A scan is already ongoing</exception>
        /// <remarks>
        /// Only one scan can be used at a time.
        /// Use <see cref="IsScanning"/> to determine if one is currently ongoing
        /// </remarks>
        private void GatherRoms()
        {
            using var scope = provider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<RomsContext>();
            try
            {
                var configs = GetConfig().ToList();
                var names = configs.Select(m => m.ShortName).ToList();
                logger.LogInformation("Configured rom directories: {Dirs}", names);
                var platforms = ctx.Platforms
                    .Include(m => m.Roms)
                    .ThenInclude(m => m.SaveStates)
                    .AsNoTracking()
                    .ToList();

                //Delete platforms that do not exist anymore
                var toDelete = platforms
                    .Where(m => !names.Contains(m.ShortName))
                    .ToList();
                if (toDelete.Count > 0)
                {
                    logger.LogInformation("Deleting {Count} platforms", toDelete.Count);
                    DeleteOldPlatforms(ctx, toDelete);
                    toDelete.ForEach(m => platforms.Remove(m));
                    toDelete.ForEach(m => names.Remove(m.ShortName));
                    UpdateMenu();
                }

                //Add new platforms and update existing ones
                foreach (var config in configs)
                {
                    CheckAbort();
                    var match = platforms.FirstOrDefault(m => m.ShortName.Equals(config.ShortName, StringComparison.InvariantCultureIgnoreCase));
                    if (match == null)
                    {
                        logger.LogInformation("Adding new platform: {Name}", config.DisplayName);
                        AddPlatform(ctx, config);
                    }
                    else
                    {
                        logger.LogInformation("Checking platform for updates: {Name}", config.DisplayName);
                        UpdatePlatform(ctx, config, match);
                    }
                    UpdateMenu();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ROM gathering service ran into an error");
            }
            finally
            {
                t = null;
                cts = null;
            }
        }

        private RomDirConfig[] GetConfig()
        {
            return File
                .ReadAllText(Path.Combine(rootDir, "config.json"))
                .FromJsonRequired<RomDirConfig[]>();
        }

        private int AddPlatform(RomsContext ctx, RomDirConfig config)
        {
            var p = new Platform()
            {
                DisplayName = config.DisplayName,
                ShortName = config.ShortName,
                EmulatorType = config.EmulatorType,
                Folder = config.FolderName,
                Roms = [],
            };
            ctx.Platforms.Add(p);
            //Roms
            foreach (var romFile in GetRomFiles(p))
            {
                CheckAbort();
                var rf = UpdateRomInfo(p, new(), romFile);
                rf.Validate();
                ctx.RomFiles.Add(rf);
            }
            return ctx.SaveChanges();
        }

        private RomFile UpdateRomInfo(Platform p, RomFile f, string romFile)
        {
            if (f.Platform != p)
            {
                f.Platform = p;
                f.PlatformId = p.Id;
            }
            f.FilePath = GetRomSubPath(GetRomPath(p), romFile);
            f.FileName = Path.GetFileName(romFile);
            f.DisplayName = Path.GetFileNameWithoutExtension(f.FileName);
            if (f.Size == 0 || GetFileSize(romFile) != f.Size)
            {
                logger.LogInformation("(Re-)Computing hash of {FileName}", f.FilePath);
                using var fs = File.OpenRead(romFile);
                f.Sha256 = SHA256.HashData(fs);
                f.Size = (int)fs.Position;
            }
            return f;
        }

        private static string GetRomSubPath(string rootPath, string romFile)
        {
            if (!romFile.StartsWith(rootPath + Path.DirectorySeparatorChar))
            {
                throw new ArgumentException($"Rom path '{romFile}' is not a subdirectory of '{rootPath}'");
            }
            return romFile[(rootPath.Length + 1)..];
        }

        private static int GetFileSize(string file) => (int)new FileInfo(file).Length;

        private string GetRomPath(Platform p) => Path.Combine(rootDir, p.Folder);

        private IEnumerable<string> GetRomFiles(Platform p)
        {
            return Directory.EnumerateFiles(Path.Combine(rootDir, p.Folder), "*.*", SearchOption.AllDirectories);
        }

        private int UpdatePlatform(RomsContext ctx, RomDirConfig config, Platform p)
        {
            ctx.Platforms.Attach(p);
            p.ShortName = config.ShortName;
            p.EmulatorType = config.EmulatorType;
            p.DisplayName = config.DisplayName;
            p.Folder = config.FolderName;

            var pending = p.Roms.ToList();
            var romPath = GetRomPath(p);
            foreach (var romFile in GetRomFiles(p))
            {
                CheckAbort();
                var subPath = GetRomSubPath(romPath, romFile);
                var existing = pending.FirstOrDefault(m => m.FilePath == subPath);
                if (existing != null)
                {
                    pending.Remove(existing);
                    //Update existing ROM
                    ctx.RomFiles.Attach(existing);
                    UpdateRomInfo(p, existing, romFile);
                    existing.Validate();
                }
                else
                {
                    //Add new ROM
                    var rf = UpdateRomInfo(p, new(), romFile);
                    rf.Validate();
                    ctx.RomFiles.Add(rf);
                }
            }
            //Delete ROMS that no longer exist
            ctx.SaveStates.RemoveRange(pending.SelectMany(m => m.SaveStates));
            ctx.RomFiles.RemoveRange(pending);
            return ctx.SaveChanges();
        }

        private int DeleteOldPlatforms(RomsContext ctx, IEnumerable<Platform> platforms)
        {
            int count = 0;
            foreach (var platform in platforms)
            {
                CheckAbort();
                logger.LogInformation("Deleting {Count} ROMs from {Name}...", platform.Roms.Count, platform.DisplayName);
                ctx.SaveStates.RemoveRange(platform.Roms.SelectMany(m => m.SaveStates));
                ctx.RomFiles.RemoveRange(platform.Roms);
                ctx.Platforms.Remove(platform);
                count += ctx.SaveChanges();
            }
            logger.LogInformation("Done. Deleted {Count} rows", count);
            return count;
        }
    }
}
