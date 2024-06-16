using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Services;

namespace RomsBrowse.Web.Controllers
{
    public class RomController(RomSearchService searchService) : Controller
    {
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var p = await searchService.GetRomPath(id);
            if (p == null)
            {
                return NotFound();
            }
            var fs = System.IO.File.OpenRead(p);
            return File(fs, "application/octet-stream");
        }
    }
}
