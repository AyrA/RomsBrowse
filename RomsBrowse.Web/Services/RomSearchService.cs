using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;
using System.Text.RegularExpressions;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public partial class RomSearchService(IConfiguration config, RomsContext ctx)
    {
        private const int MaxResultCount = 100;

        private readonly string rootDir = Path.GetFullPath(config.GetValue<string>("RomDir")
            ?? throw new InvalidOperationException("'RomDir' not set"));


        public int ResultLimit => MaxResultCount;

        public async Task<RomFile[]> Search(string terms)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(terms);
            terms = ParseString(terms);
            if (string.IsNullOrWhiteSpace(terms))
            {
                return [];
            }
            return await ctx.RomFiles
                .Include(m => m.Platform)
                .Where(m => EF.Functions.Contains(m.DisplayName, terms))
                .AsNoTracking()
                .OrderBy(m => m.DisplayName)
                .Take(ResultLimit)
                .ToArrayAsync();
        }

        public async Task<RomFile[]> Search(string terms, int platformId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(terms);
            terms = ParseString(terms);
            if (string.IsNullOrWhiteSpace(terms))
            {
                return [];
            }
            return await ctx.RomFiles
                .Where(m => m.PlatformId == platformId && EF.Functions.FreeText(m.DisplayName, terms))
                .AsNoTracking()
                .OrderBy(m => m.DisplayName)
                .Take(ResultLimit)
                .ToArrayAsync();
        }

        public async Task<RomFile?> GetRom(int id)
        {
            return await ctx.RomFiles
                .AsNoTracking()
                .Include(m => m.Platform)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<string?> GetRomPath(int id, string? fileName = null)
        {
            var rom = await GetRom(id);
            if (rom == null || (fileName != null && rom.FileName != fileName))
            {
                return null;
            }
            return Path.Combine(rootDir, rom.Platform.Folder, rom.FilePath);
        }

        private static string ParseString(string term)
        {
            //Replace SQL search values with spaces
            term = SqlReplacer().Replace(term, " ");
            //Split on spaces
            var terms = WhitespaceSplitter().Split(term).Select(m => m.Trim());
            //Join with SQL "AND" value
            term = string.Join("&", terms);

            //If the string is fully whitespace or too short, discard term and don't search
            if (term.Length < 3 || WSOnlyChecker().IsMatch(term))
            {
                return string.Empty;
            }

            return term;
        }

        [GeneratedRegex(@"[&|~!]+")]
        private static partial Regex SqlReplacer();

        [GeneratedRegex(@"^\s*$")]
        private static partial Regex WSOnlyChecker();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceSplitter();
    }
}
