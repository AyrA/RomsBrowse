using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Data;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    public class InitController(RomsContext ctx, SetupService ss) : Controller
    {
        public IActionResult Index()
        {
            if (ss.IsConfigured)
            {
                return RedirectToAction("Index", "Home");
            }
            var vm = new DbSetupViewModel()
            {
                UseWindowsAuth = true,
                Encrypt = false
            };
            return View(vm);
        }
    }
}
