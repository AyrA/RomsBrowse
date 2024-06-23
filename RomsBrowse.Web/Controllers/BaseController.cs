using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
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

        protected string CurrentUrl => HttpContext.Request.GetEncodedPathAndQuery();

        protected string? ReturnUrl
        {
            get
            {
                var url = HttpContext.Request.Query["returnUrl"].FirstOrDefault();
                if (url != null)
                {
                    try
                    {
                        return new Uri(new Uri("http://localhost/"), url).PathAndQuery;
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
        }

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

        /// <summary>
        /// Redirect to login page,
        /// adding information to redirect back to the current location into the URL
        /// </summary>
        /// <returns>Redirection</returns>
        protected RedirectToActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account", new { returnUrl = CurrentUrl });
        }

        /// <summary>
        /// Redirects back to the action where <see cref="RedirectToLogin"/>
        /// was previously called.
        /// </summary>
        /// <returns>Redirection</returns>
        protected RedirectResult RedirectBack()
        {
            return Redirect(ReturnUrl ?? "/");
        }

        protected void SetErrorMessage(string message)
        {
            ViewData["ErrorMessage"] = message;
        }

        protected void SetErrorMessage(Exception exception) => SetErrorMessage(exception.Message);
    }
}
