using Microsoft.AspNetCore.Mvc;

namespace RomsBrowse.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected void SetErrorMessage(string message)
        {
            ViewData["ErrorMessage"] = message;
        }

        protected void SetErrorMessage(Exception exception) => SetErrorMessage(exception.Message);
    }
}
