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
                UseWindowsAuth = true,
                Encrypt = false
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Index(DbSetupViewModel model)
        {
            if (ss.IsConfigured)
            {
                return RedirectToAction("Index", "Home");
            }
            try
            {
                model.Validate();
                ss.SetConnectionString(model.GetConnectionString(), model.ProviderString);
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
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(false, GetErrorString(ex)));
            }
            ctx.ConnectionString = model.GetConnectionString();
            try
            {
                ctx.TestConnection();
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(false, GetErrorString(ex)));
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
