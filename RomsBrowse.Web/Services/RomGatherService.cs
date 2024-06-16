using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.ServiceModels;
using System.Security.Cryptography;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class RomGatherService(IConfiguration config, RomsContext ctx, ILogger<RomGatherService> logger)
    {
        private readonly string rootDir = Path.GetFullPath(config.GetValue<string>("RomDir")
            ?? throw new InvalidOperationException("'RomDir' not set"));

        private RomDirConfig[] GetConfig()
        {
            return File
                .ReadAllText(Path.Combine(rootDir, "config.json"))
                .FromJsonRequired<RomDirConfig[]>();
        }

        public async Task GatherRoms()
        {
            var configs = GetConfig().ToList();
            var names = configs.Select(m => m.ShortName).ToList();
            logger.LogInformation("Configured rom directories: {Dirs}", names);
            var platforms = await ctx.Platforms
                .Include(m => m.Roms)
                .AsNoTracking()
                .ToListAsync();

            //Delete platforms that do not exist anymore
            var toDelete = platforms
                .Where(m => !names.Contains(m.ShortName))
                .ToList();
            if (toDelete.Count > 0)
            {
                logger.LogInformation("Deleting {Count} platforms", toDelete.Count);
                await DeleteOldPlatforms(toDelete);
                toDelete.ForEach(m => platforms.Remove(m));
                toDelete.ForEach(m => names.Remove(m.ShortName));
            }

            //Add new platforms and update existing ones
            foreach (var config in configs)
            {
                var match = platforms.FirstOrDefault(m => m.ShortName.Equals(config.ShortName, StringComparison.InvariantCultureIgnoreCase));
                if (match == null)
                {
                    logger.LogInformation("Adding new platform: {Name}", config.DisplayName);
                    await AddPlatform(config);
                }
                else
                {
                    logger.LogInformation("Checking platform for updates: {Name}", config.DisplayName);
                    await UpdatePlatform(config, match);
                }
            }
        }

        private async Task<int> AddPlatform(RomDirConfig config)
        {
            var p = new Platform()
            {
                DisplayName = config.DisplayName,
                ShortName = config.ShortName,
                Folder = config.FolderName,
                Roms = [],
            };
            ctx.Platforms.Add(p);
            //Roms
            foreach (var romFile in GetRomFiles(p))
            {
                var rf = await UpdateRomInfoAsync(p, new(), romFile);
                rf.Validate();
                ctx.RomFiles.Add(rf);
            }
            return await ctx.SaveChangesAsync();
        }

        private async Task<RomFile> UpdateRomInfoAsync(Platform p, RomFile f, string romFile)
        {
            if (f.Platform != p)
            {
                f.Platform = p;
                f.PlatformId = p.Id;
            }
            f.FilePath = GetRomSubPath(GetRomPath(p), romFile);
            f.FileName = Path.GetFileName(romFile);
            f.DisplayName = Path.GetFileNameWithoutExtension(f.FileName);
            if (f.Size == 0 || FileSize(romFile) != f.Size)
            {
                logger.LogInformation("(Re-)Computing hash of {FileName}", f.FilePath);
                using var fs = File.OpenRead(romFile);
                f.Sha256 = await SHA256.HashDataAsync(fs);
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

        private static int FileSize(string file) => (int)new FileInfo(file).Length;

        private string GetRomPath(Platform p) => Path.Combine(rootDir, p.Folder);

        private IEnumerable<string> GetRomFiles(Platform p)
        {
            return Directory.EnumerateFiles(Path.Combine(rootDir, p.Folder), "*.*", SearchOption.AllDirectories);
        }

        private async Task<int> UpdatePlatform(RomDirConfig config, Platform p)
        {
            ctx.Platforms.Attach(p);
            p.ShortName = config.ShortName;
            p.DisplayName = config.DisplayName;
            p.Folder = config.FolderName;

            var pending = p.Roms.ToList();
            var romPath = GetRomPath(p);
            foreach (var romFile in GetRomFiles(p))
            {
                var subPath = GetRomSubPath(romPath, romFile);
                var existing = pending.FirstOrDefault(m => m.FilePath == subPath);
                if (existing != null)
                {
                    pending.Remove(existing);
                    //Update existing ROM
                    ctx.RomFiles.Attach(existing);
                    await UpdateRomInfoAsync(p, existing, romFile);
                    existing.Validate();
                }
                else
                {
                    //Add new ROM
                    var rf = await UpdateRomInfoAsync(p, new(), romFile);
                    rf.Validate();
                    ctx.RomFiles.Add(rf);
                }
            }
            //Delete ROMS that no longer exist
            ctx.RomFiles.RemoveRange(pending);
            return await ctx.SaveChangesAsync();
        }

        private async Task<int> DeleteOldPlatforms(IEnumerable<Platform> platforms)
        {
            int count = 0;
            foreach (var platform in platforms)
            {
                logger.LogInformation("Deleting {Count} ROMs from {Name}...", platform.Roms.Count, platform.DisplayName);
                ctx.RomFiles.RemoveRange(platform.Roms);
                ctx.Platforms.Remove(platform);
                count += await ctx.SaveChangesAsync();
            }
            logger.LogInformation("Done. Deleted {Count} rows", count);
            return count;
        }
    }
}
