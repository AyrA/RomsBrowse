using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class RomViewModel(RomFile rom)
    {
        public string EmulatorView { get; set; } = rom.Platform.ShortName;
        public string Title { get; set; } = rom.DisplayName;
        public int Id { get; set; } = rom.Id;
        public string EmuType { get; set; } = rom.Platform.EmulatorType;
    }
}
