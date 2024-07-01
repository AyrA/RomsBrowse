using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Text;

namespace RomsBrowse.Web.Controllers
{
    public class RomController(UserService userService, RomSearchService searchService, SettingsService settingsService, SaveService saveStateService, EmulatorCachingService emuCache) : BaseController(userService)
    {
        [Route("{controller}/{action}/{id}/{fileName}")]
        public async Task<IActionResult> Get(int id, string fileName)
        {
            if (!CanPlay())
            {
                return new StatusCodeResult(401);
            }
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
            if (!CanPlay())
            {
                return RedirectToLogin();
            }
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

        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> EmulatorScript(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            if (!CanPlay())
            {
                return new StatusCodeResult(401);
            }
            var rom = await searchService.GetRom(id);
            if (rom == null)
            {
                return NotFound();
            }
            var hasState = await saveStateService.HasState(id, UserName!);
            //rom.FileName is added separately to not escape its characters.
            //Since it's a valid file name, no escaping should be necessary.
            //Strictly speaking, the file name is not necessary since ids are used to identify items.
            //But having the game name at the end makes the emulator shows them in settings rather than the id,
            //which is easier for users to work with.
            //The name is still validated in the Get function above to stop users from messing with this value.
            var gameUrl = Url.Action("Get", "Rom", new { id = rom.Id }) + "/" + rom.FileName;
            var stateUrl = hasState ? $"/Rom/GetState/{rom.Id}" : null;
            var romCode = @$"""use strict;""
function setEmulatorInitValues(){{
    window.EJS_isSignedIn     = {IsLoggedIn.ToJson()};
    window.EJS_player         = ""#game"";
    window.EJS_core           = {rom.Platform.EmulatorType.ToJson()};
    window.EJS_biosUrl        = {string.Empty.ToJson()};
    window.EJS_gameUrl        = {gameUrl.ToJson()};
    window.EJS_pathtodata     = ""/emulator/data"";
    window.EJS_gameName       = {rom.DisplayName.ToJson()};
    window.EJS_loadStateURL   = {stateUrl.ToJson()};
    window.EJS_defaultOptions = {{
        'save-state-location': 'keep in browser'
    }};
}}

if (WasmCheck.reportWebAssembly()) {{
    setEmulatorInitValues();
    EmulatorInterop.startEmulator();
}}

";
            return File(Encoding.UTF8.GetBytes(romCode), "text/javascript");
        }

        public async Task<IActionResult> SaveState(SaveStateModel model)
        {
            if (!CanPlay())
            {
                HttpContext.Response.StatusCode = 401;
                return Json(new { Success = false, Error = "Saving progrsss on the server requires an active session" });
            }
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                HttpContext.Response.StatusCode = 400;
                return Json(new { Success = false, Error = ex.Message });
            }

            using var img = model.Screenshot.OpenReadStream();
            using var data = model.SaveState.OpenReadStream();

            try
            {
                await saveStateService.SaveData(model.GameId, UserName!, img, data);
            }
            catch (Exception ex)
            {
                HttpContext.Response.StatusCode = 500;
                return Json(new { Success = false, Error = ex.Message });
            }

            return Json(new { Success = true });
        }

        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> GetState(int Id)
        {
            if (!CanPlay())
            {
                return new StatusCodeResult(401);
            }
            if (Id < 1)
            {
                return NotFound();
            }
            var state = await saveStateService.GetState(Id, UserName!);
            if (state == null)
            {
                return NotFound();
            }

            return File(state.Data, "application/octet-stream");
        }

        public async Task<IActionResult> SaveSRAM(SaveSRAMModel model)
        {
            if (!CanPlay())
            {
                HttpContext.Response.StatusCode = 401;
                return Json(new { Success = false, Error = "Saving progrsss on the server requires an active session" });
            }
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                HttpContext.Response.StatusCode = 400;
                return Json(new { Success = false, Error = ex.Message });
            }

            using var data = model.SaveState.OpenReadStream();
            using var image = model.Screenshot.OpenReadStream();

            try
            {
                await saveStateService.SaveSRAM(model.GameId, UserName!, image, data);
            }
            catch (Exception ex)
            {
                HttpContext.Response.StatusCode = 500;
                return Json(new { Success = false, Error = ex.Message });
            }

            return Json(new { Success = true });
        }

        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> GetSRAM(int Id)
        {
            if (!CanPlay())
            {
                return new StatusCodeResult(401);
            }
            if (Id < 1)
            {
                return NotFound();
            }
            var sramData = await saveStateService.GetSRAM(Id, UserName!);
            if (sramData == null)
            {
                return NotFound();
            }

            return File(sramData.Data, "application/octet-stream");
        }

        public bool CanPlay()
        {
            return IsLoggedIn || settingsService.GetValue<bool>(SettingsService.KnownSettings.AnonymousPlay);
        }
    }
}
