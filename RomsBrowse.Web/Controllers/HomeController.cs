using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Diagnostics;

namespace RomsBrowse.Web.Controllers
{
    public class HomeController(RomSearchService searchService, PlatformService platformService) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SearchAsync([FromQuery] SearchViewModel model)
        {
            model.Validate();
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

        public async Task<IActionResult> Rom(int? id)
        {
            if (id == null || id.Value <= 0)
            {
                return NotFound();
            }
            var rom = await searchService.GetRom(id.Value);
            if (rom == null)
            {
                return NotFound();
            }
            var rvm = new RomViewModel(rom);
            return View(rvm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
