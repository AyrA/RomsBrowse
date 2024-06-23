using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserService _userService;
        private readonly SettingsService _settingsService;

        public AccountController(UserService userService, SettingsService settingsService) : base(userService)
        {
            _userService = userService;
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (IsLoggedIn)
            {
                return RedirectBack();
            }
            if (!await CanCreateAccount())
            {
                return RedirectToAction("RegisterDisabled");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (IsLoggedIn)
            {
                return RedirectBack();
            }
            if (!await CanCreateAccount())
            {
                return RedirectToAction("RegisterDisabled");
            }
            model.UserCreated = false;
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
                return RedirectBack();
            }
            return View(new SignInViewModel() { RedirectUrl = ReturnUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(SignInViewModel model)
        {
            if (IsLoggedIn)
            {
                return RedirectBack();
            }
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
                return View(model);
            }
            await _userService.Ping(verify.Username);
            await HttpContext.SignInAsync(_userService.GetPrincipal(verify.Username));
            return RedirectBack();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectBack();
        }

        private async Task<bool> CanCreateAccount()
        {
            return _settingsService.GetValue<bool>(SettingsService.KnownSettings.AllowRegister)
                || !await _userService.HasAdmin();
        }
    }
}
