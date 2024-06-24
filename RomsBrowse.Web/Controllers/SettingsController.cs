using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Data.Enums;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    [Authorize(Roles = nameof(UserFlags.Admin))]
    public class SettingsController(UserService userService, SettingsService ss) : BaseController(userService)
    {
        public IActionResult Index()
        {
            var vm = new SettingsViewModel();
            return View(vm);
        }
    }
}
