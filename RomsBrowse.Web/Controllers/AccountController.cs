using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    public class AccountController : BaseController
    {
        private static readonly CookieOptions userIdOpt = new()
        {
            HttpOnly = true,
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(365),
            Path = "/",
            SameSite = SameSiteMode.Strict,
#if !DEBUG
            Secure = true
#endif
        };

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
        public IActionResult Login(SignInModel model)
        {
            if (model.Id == Guid.Empty)
            {
                return BadRequest("Invalid or empty user id. Please try again");
            }
            else
            {
                HttpContext.Response.Cookies.Append("UserId", model.Id.ToString(), userIdOpt);
            }
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("UserId", userIdOpt);
            return Ok();
        }
    }
}
