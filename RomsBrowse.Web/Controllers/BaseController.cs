using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Services;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Text;

namespace RomsBrowse.Web.Controllers
{
    public abstract class BaseController(UserService userService) : Controller
    {
        private const string MessageCookieKey = "RedirectMessage";
        private static ITempEncryptionService? _encryptionService;
        private static SetupService? _setupService;
        private UserViewModel? _currentUser;

        /// <summary>
        /// Gets the user name of the currently logged in user
        /// </summary>
        /// <remarks><see cref="IsLoggedIn"/> should be checked first</remarks>
        protected string? UserName => User.Identity?.Name;

        protected UserViewModel? CurrentUser => _currentUser;

        /// <summary>
        /// Gets if the user is logged in
        /// </summary>
        protected bool IsLoggedIn => User.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// Gets the URL the user currently requested, including query parameter
        /// </summary>
        protected string CurrentUrl => HttpContext.Request.GetEncodedPathAndQuery();

        /// <summary>
        /// Gets the "returnUrl" query param value, if there is one
        /// </summary>
        /// <remarks>
        /// This will ensure that the URL is relative,
        /// cutting off any origin that may be present
        /// </remarks>
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
            if (_setupService == null)
            {
                throw new InvalidOperationException("SetupService has not been set");
            }
            if (!_setupService.IsConfigured)
            {
                context.Result = _setupService.SetupRedirect;
            }
            else
            {
                var user = userService.Get(UserName!, false);
                if (IsLoggedIn && (user == null || !user.CanSignIn))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Result = new RedirectResult("/");
                }
                ViewData["CurrentUrl"] = CurrentUrl;
                ViewData["User"] = _currentUser = new UserViewModel(user);
                ViewData["HasAdmin"] = await userService.HasAdmin();

                if (_encryptionService != null && Request.Cookies.TryGetValue(MessageCookieKey, out var redirMessage))
                {
                    Response.Cookies.Delete(MessageCookieKey);
                    try
                    {
                        var data = Encoding.UTF8.GetString(_encryptionService.Decrypt(Convert.FromBase64String(redirMessage))).FromJson<string[]>();
                        if (data != null && data.Length == 2)
                        {
                            if (data[1] == bool.TrueString)
                            {
                                SetSuccessMessage(data[0]);
                            }
                            else
                            {
                                SetErrorMessage(data[0]);
                            }
                        }
                    }
                    catch
                    {
                        //NOOP
                    }
                }
            }
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

        public static void SetStaticServices(ITempEncryptionService encryptionService, SetupService setupService)
        {
            _encryptionService = encryptionService;
            _setupService = setupService;
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

        protected void SetRedirectMessage(string message, bool isSuccess)
        {
            if (_encryptionService == null)
            {
                throw new InvalidOperationException("Encryption service not initialized");
            }
            var serialized = new string[] { message, isSuccess ? bool.TrueString : bool.FalseString }.ToJson();
            var cookieValue = Convert.ToBase64String(_encryptionService.Encrypt(Encoding.UTF8.GetBytes(serialized)));
            Response.Cookies.Append(MessageCookieKey, cookieValue);
        }

        protected void SetRedirectMessage(Exception ex) => SetRedirectMessage(ex.Message, false);

        protected void SetSuccessMessage(string message)
        {
            ViewData.Remove("ErrorMessage");
            ViewData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            ViewData.Remove("SuccessMessage");
            ViewData["ErrorMessage"] = message;
        }

        protected void SetErrorMessage(Exception exception) => SetErrorMessage(exception.Message);
    }
}
