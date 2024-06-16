using AyrA.AutoDI;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class MainMenuService
    {
        public PlatformMenuModel[] Platforms { get; private set; } = [];

        public void SetMenuItems(IEnumerable<Platform> platforms)
        {
            Platforms = platforms
                .OrderBy(m => m.DisplayName)
                .Select(m => new PlatformMenuModel(m.DisplayName, m.Id))
                .ToArray();
        }
    }
}
