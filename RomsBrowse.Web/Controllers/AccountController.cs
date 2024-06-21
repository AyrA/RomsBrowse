using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.ViewModels;
using System.Security.Claims;

namespace RomsBrowse.Web.Controllers
{
    public class AccountController : BaseController
    {
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
            if (model.Id == Guid.Empty)
            {
                return BadRequest("Invalid or empty user id. Please try again");
            }
            else
            {
                var claim = new Claim(ClaimTypes.Name, model.Id.ToString());
                var ident = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                ident.AddClaim(claim);
                var cp = new ClaimsPrincipal(ident);
                await HttpContext.SignInAsync(cp);
            }
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
