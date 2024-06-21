using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected Guid UserId { get; private set; }

        protected bool IsLoggedIn => UserId != Guid.Empty;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cookie = context.HttpContext.Request.Cookies["UserId"];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                if (Guid.TryParse(cookie, out Guid value))
                {
                    UserId = value;
                }
            }
            ViewData["User"] = new UserViewModel(UserId);
            base.OnActionExecuting(context);
        }

        protected void SetErrorMessage(string message)
        {
            ViewData["ErrorMessage"] = message;
        }

        protected void SetErrorMessage(Exception exception) => SetErrorMessage(exception.Message);
    }
}
