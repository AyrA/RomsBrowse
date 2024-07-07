using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Data;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    public class InitController(SqlServerTestContext ctx, SetupService ss) : Controller
    {
        private record ApiResult(bool Success, string? Error);

        [HttpGet]
        public IActionResult Index()
        {
            if (ss.IsConfigured)
            {
                return RedirectToAction("Index", "Home");
            }
            var vm = new DbSetupViewModel()
            {
                DefaultDirectory = ss.DataDirectory,
                Provider = DbProvider.None,
                //Prefill defaults
                UseWindowsAuth = true,
                Encrypt = false,
                FileName = "roms.db3",
                ServerInstance = @".\SQLEXPRESS",
                DatabaseName = "roms"
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(DbSetupViewModel model)
        {
            if (ss.IsConfigured)
            {
                return RedirectToAction("Index", "Home");
            }
            try
            {
                model.Validate();
                model.VerifySqliteFileName(ss.DataDirectory);
                ss.SetConnectionString(model.GetConnectionString(), model.ProviderString);
                await ss.FullInit();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
            }
            model.ClearSensitiveData();
            return View(model);
        }

        public IActionResult Test(DbSetupViewModel model)
        {
            try
            {
                model.Validate();
                model.VerifySqliteFileName(ss.DataDirectory);
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(false, GetErrorString(ex)));
            }
            if (model.Provider == DbProvider.SQLServer)
            {
                ctx.ConnectionString = model.GetConnectionString();
                try
                {
                    ctx.TestConnection();
                }
                catch (Exception ex)
                {
                    return Json(new ApiResult(false, GetErrorString(ex)));
                }
            }
            else if (model.Provider == DbProvider.SQLite)
            {
                //NOOP. Model already checked file,
                //and this is enough for SQLite
            }
            return Json(new ApiResult(true, null));
        }

        private static string GetErrorString(Exception? ex)
        {
            var lines = new List<string>();
            while (ex != null)
            {
                lines.Add(ex.Message);
                ex = ex.InnerException;
            }
            return string.Join(", ", lines);
        }
    }
}
