using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Security.Claims;

namespace RomsBrowse.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserService _userService;

        public AccountController(UserService userService) : base(userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn)
            {
                return Redirect("/");
            }
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(SignInModel model)
        {
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
                return View();
            }
            if (!await _userService.VerifyAccount(model.Username, model.Password))
            {
                SetErrorMessage("Invalid username or password");
                return View();
            }
            var claim = new Claim(ClaimTypes.Name, model.Username);
            var ident = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            ident.AddClaim(claim);
            var cp = new ClaimsPrincipal(ident);
            await HttpContext.SignInAsync(cp);
            return Redirect("/");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
