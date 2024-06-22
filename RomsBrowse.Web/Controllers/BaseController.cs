using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RomsBrowse.Common.Interfaces;
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

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //Clear sensitive data before returning the model to the view or json
            ISensitiveData? model = null;

            if (context.Result is ViewResult view)
            {
                if (view.Model is ISensitiveData viewModel)
                {
                    model = viewModel;
                }
            }
            if (context.Result is JsonResult js)
            {
                if (js.Value is ISensitiveData jsModel)
                {
                    model = jsModel;
                }
            }
            model?.ClearSensitiveData();
            base.OnActionExecuted(context);
        }

        protected void SetErrorMessage(string message)
        {
            ViewData["ErrorMessage"] = message;
        }

        protected void SetErrorMessage(Exception exception) => SetErrorMessage(exception.Message);
    }
}
