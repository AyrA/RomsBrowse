using AyrA.AutoDI;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.ServiceModels;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class MainMenuService
    {
        public bool HasPlatforms => Platforms.Length > 0;

        public PlatformMenuModel[] Platforms { get; private set; } = [];

        public void SetMenuItems(IEnumerable<Platform> platforms, PlatformCountModel[] romCounts)
        {
            Platforms = platforms
                .OrderBy(m => m.DisplayName)
                .Select(m => new PlatformMenuModel(m.DisplayName, m.Id, romCounts.FirstOrDefault(n => n.PlatformId == m.Id)?.RomCount ?? 0))
                .ToArray();
        }
    }
}
