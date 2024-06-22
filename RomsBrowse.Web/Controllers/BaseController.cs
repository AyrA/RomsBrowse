using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    public abstract class BaseController(UserService userService) : Controller
    {
        protected string? UserName => User.Identity?.Name;

        protected bool IsLoggedIn => User.Identity?.IsAuthenticated ?? false;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (IsLoggedIn && !await userService.Exists(UserName!))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Result = new RedirectResult("/");
            }
            ViewData["User"] = new UserViewModel(UserName);
            await base.OnActionExecutionAsync(context, next);
        }

        protected void SetErrorMessage(string message)
        {
            ViewData["ErrorMessage"] = message;
        }

        protected void SetErrorMessage(Exception exception) => SetErrorMessage(exception.Message);
    }
}
