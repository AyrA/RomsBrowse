using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Data;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Diagnostics;

namespace RomsBrowse.Web.Controllers
{
    public class HomeController(UserService userService, RomSearchService searchService, PlatformService platformService, RomsContext ctx) : BaseController(userService)
    {
        public IActionResult Index()
        {
            if (!ctx.IsConfigured)
            {
                return RedirectToAction("Index", "Init");
            }
            return View();
        }

        public async Task<IActionResult> Search([FromQuery] SearchViewModel model)
        {
            try
            {
                model.Validate();
                model.Search = model.Search.Trim();
            }
            catch
            {
                SetErrorMessage("Did not find any items");
                return View("Index");
            }
            var vm = new SearchResultViewModel()
            {
                SearchModel = model
            };
            if (model.Platform != null)
            {
                vm.Files = await searchService.Search(model.Search, model.Platform.Value);
            }
            else
            {
                vm.Files = await searchService.Search(model.Search);
            }
            vm.IsLimited = vm.Files.Length >= searchService.ResultLimit;
            if (vm.Files.Length == 0)
            {
                SetErrorMessage("Did not find any items");
            }

            return View(vm);
        }

        public async Task<IActionResult> Platform(int id, int page)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var platform = await platformService.GetPlatform(id, false);
            if (platform == null)
            {
                return NotFound();
            }
            page = Math.Max(page, 1);
            var roms = await searchService.GetRoms(id, page);
            var totalCount = await platformService.GetRomCount(id);
            var vm = new PlatformViewModel()
            {
                PlatformName = platform.DisplayName,
                RomCount = totalCount,
                Page = page,
                PageCount = (int)Math.Ceiling((double)totalCount / searchService.ResultLimit),
                Roms = roms,
            };
            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
