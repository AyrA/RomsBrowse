using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Text;

namespace RomsBrowse.Web.Controllers
{
    public class RomController(RomSearchService searchService, EmulatorCachingService emuCache) : Controller
    {
        [Route("{controller}/{action}/{id}/{fileName}")]
        public async Task<IActionResult> Get(int id, string fileName)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }
            var p = await searchService.GetRomPath(id, fileName);
            if (p == null)
            {
                return NotFound();
            }
            var fs = System.IO.File.OpenRead(p);
            return File(fs, "application/octet-stream");
        }

        public async Task<IActionResult> Play(int? id)
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

            var rvm = new RomPlayViewModel(rom)
            {
                HasEmulator = emuCache.HasEmulator,
                IsDownloadingEmulator = emuCache.IsDownloading
            };
            return View(rvm);
        }

        public async Task<IActionResult> EmulatorScript(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var rom = await searchService.GetRom(id);
            if (rom == null)
            {
                return NotFound();
            }
            //rom.FileName is added separately to not escape its characters.
            //Since it's a valid file name, no escaping should be necessary
            var url = Url.Action("Get", "Rom", new { id = rom.Id }) + "/" + rom.FileName;
            var romCode = @$"EJS_player = ""#game"";
EJS_core = {rom.Platform.EmulatorType.ToJson()};
EJS_biosUrl = {string.Empty.ToJson()};
EJS_gameUrl = {url.ToJson()};
EJS_pathtodata = ""/emulator/data"";
EJS_gameName = {rom.DisplayName.ToJson()};
EJS_onSaveState = function(e) {{console.log(e);}};
EJS_defaultOptions = {{
    'save-state-location': 'keep in browser'
}};
";
            return File(Encoding.UTF8.GetBytes(romCode), "text/javascript");
        }
    }
}
