using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public partial class RomSearchService(SettingsService ss, RomsContext ctx)
    {
        private const int MaxResultCount = 100;

        private readonly string? rootDir = ss.GetValue<string>(SettingsService.KnownSettings.RomDirectory);

        public int ResultLimit => MaxResultCount;

        [MemberNotNull(nameof(rootDir))]
        private void EnsureRoot()
        {
            if (string.IsNullOrWhiteSpace(rootDir))
            {
                throw new InvalidOperationException("ROM root directory has not been set");
            }
        }

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
            EnsureRoot();
            var rom = await GetRom(id);
            if (rom == null || (fileName != null && rom.FileName != fileName))
            {
                return null;
            }
            return Path.Combine(rootDir, rom.Platform.Folder, rom.FilePath);
        }

        public async Task<RomFile[]> GetRoms(int platformId, int page)
        {
            page = Math.Max(1, page);
            if (platformId <= 0)
            {
                return [];
            }
            return await ctx.RomFiles
                .AsNoTracking()
                .Where(m => m.PlatformId == platformId)
                .OrderBy(m => m.DisplayName)
                .Skip((page - 1) * MaxResultCount)
                .Take(MaxResultCount)
                .ToArrayAsync();
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
