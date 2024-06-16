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
            var config = GetConfig().ToList();
            var names = config.Select(m => m.ShortName).ToList();
            logger.LogInformation("Configured rom directories: {Dirs}", names);
            var platforms = await ctx.Platforms
                .Include(m => m.Roms)
                .AsNoTracking()
                .ToListAsync();

            //Delete platforms that do not exist anymore
            var toDelete = platforms
                .Where(m => names.Contains(m.ShortName))
                .ToList();
            if (toDelete.Count > 0)
            {
                logger.LogInformation("Deleting {Count} platforms", toDelete.Count);
                await DeleteOldPlatforms(toDelete);
                toDelete.ForEach(m => platforms.Remove(m));
                toDelete.ForEach(m => names.Remove(m.ShortName));
            }

            //Add new platforms and update existing ones
            foreach (var p in config)
            {
                if (!platforms.Any(m => m.ShortName.Equals(p.ShortName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    logger.LogInformation("Adding new platform: {Name}", p.DisplayName);
                    await AddPlatform(p);
                }
                else
                {
                    logger.LogInformation("Checking platform for updates: {Name}", p.DisplayName);
                    await UpdatePlatform(p);
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
            f.FilePath = romFile[(GetRomPath(p).Length + 1)..];
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

        private static int FileSize(string file) => (int)new FileInfo(file).Length;

        private string GetRomPath(Platform p) => Path.Combine(rootDir, p.Folder);

        private IEnumerable<string> GetRomFiles(Platform p)
        {
            return Directory.EnumerateFiles(Path.Combine(rootDir, p.Folder), "*.*", SearchOption.AllDirectories);
        }

        private async Task<int> UpdatePlatform(RomDirConfig config)
        {
            throw new NotImplementedException();
            //TODO
            //return await ctx.SaveChangesAsync();
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
