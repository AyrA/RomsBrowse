using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Diagnostics;

namespace RomsBrowse.Web.Controllers
{
    public class HomeController(RomSearchService searchService) : Controller
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

        public async Task<IActionResult> ROM(int? id)
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
