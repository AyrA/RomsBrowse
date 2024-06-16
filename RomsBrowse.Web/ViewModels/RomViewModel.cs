using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public record RomViewModel(string EmulatorCore, RomFile Rom);
}
