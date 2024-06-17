using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class PlatformViewModel
    {
        public string PlatformName { get; set; } = string.Empty;
        public int RomCount { get; set; }
        public int Page { get; set; }
        public RomFile[] Roms { get; set; } = [];
        public int PageCount { get; set; }
    }
}
