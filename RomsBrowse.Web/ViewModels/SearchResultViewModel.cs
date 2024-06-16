using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class SearchResultViewModel
    {
        public SearchViewModel SearchModel { get; set; } = new();
        public RomFile[] Files { get; set; } = [];
        public bool IsLimited { get; set; }
    }
}
