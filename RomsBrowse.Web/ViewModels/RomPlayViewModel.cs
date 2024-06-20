using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class RomPlayViewModel(RomFile rom)
    {
        public bool IsDownloadingEmulator { get; set; }
        public bool HasEmulator { get; set; }
        public string EmulatorView { get; set; } = rom.Platform.ShortName;
        public string Title { get; set; } = rom.DisplayName;
        public string FileName { get; set; } = rom.FileName;
        public int Id { get; set; } = rom.Id;
        public string EmuType { get; set; } = rom.Platform.EmulatorType;
    }
}
