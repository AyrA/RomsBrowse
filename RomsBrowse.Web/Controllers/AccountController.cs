using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;

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
        public IActionResult Register()
        {
            if (IsLoggedIn)
            {
                return Redirect("/");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            model.UserCreated = false;
            if (IsLoggedIn)
            {
                return Redirect("/");
            }
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
                return View(model);
            }
            if (await _userService.Exists(model.Username))
            {
                SetErrorMessage("User already exists");
                return View(model);
            }
            try
            {
                if (await _userService.Create(model.Username, model.Password1))
                {
                    model.UserCreated = true;
                }
                else
                {
                    throw new Exception("Failed to create user");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
                return View(model);
            }
            return View(model);
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
            var verify = await _userService.VerifyAccount(model.Username, model.Password);
            if (!verify.IsValid)
            {
                SetErrorMessage("Invalid username or password");
                return View();
            }
            await HttpContext.SignInAsync(_userService.GetPrincipal(verify.Username));
            return Redirect("/");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }
    }
}
