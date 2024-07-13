using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Common.Services;
using RomsBrowse.Data.Enums;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;

namespace RomsBrowse.Web.Controllers
{
    public class AccountController : BaseController
    {
        private static readonly SemaphoreSlim _registerLock = new(1);

        private readonly UserService _userService;
        private readonly SettingsService _settingsService;
        private readonly IPasswordCheckerService _passwordCheckerService;
        private readonly SaveService _saveService;

        public AccountController(SaveService saveService, IPasswordCheckerService passwordCheckerService, UserService userService, SettingsService settingsService) : base(userService)
        {
            _userService = userService;
            _settingsService = settingsService;
            _passwordCheckerService = passwordCheckerService;
            _saveService = saveService;
        }

        public IActionResult Index() => RedirectToAction(nameof(Saves));

        public async Task<IActionResult> Saves()
        {
            var vm = await _saveService.GetSaves(UserName!);
            var maxSaves = _settingsService.GetValue<int>(SettingsService.KnownSettings.MaxSaveStatesPerUser);
            var maxAge = _settingsService.GetValue<TimeSpan>(SettingsService.KnownSettings.SaveStateExpiration).Days;

            vm.MaxSaves = maxSaves;
            vm.DeleteDaysBack = maxAge;

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetTimer(SaveOperationViewModel model)
        {
            try
            {
                await _saveService.ResetTimer(model.Id, UserName!, model.Type);
                SetRedirectMessage($"{model.Type} expiration timer was reset", true);
            }
            catch (Exception ex)
            {
                SetRedirectMessage(ex);
            }
            return RedirectToAction(nameof(Saves));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSave(SaveOperationViewModel model)
        {
            try
            {
                await _saveService.Delete(model.Id, UserName!, model.Type);
                SetRedirectMessage($"{model.Type} was deleted", true);
            }
            catch (Exception ex)
            {
                SetRedirectMessage(ex);
            }
            return RedirectToAction(nameof(Saves));
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
                return View("RegisterDisabled");
            }
            var vm = new RegisterViewModel()
            {
                HasAdmin = await _userService.HasAdmin()
            };
#if DEBUG
            //During debug mode, add the token
            //so we don't have to consult the db every time during testing
            if (!vm.HasAdmin)
            {
                if (Guid.TryParse(_settingsService.GetRawValue(SettingsService.KnownSettings.AdminToken), out var adminToken))
                {
                    vm.AdminToken = adminToken;
                }
            }
#endif
            return View(vm);
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
                return View("RegisterDisabled");
            }
            model.UserCreated = false;
            model.HasAdmin = await _userService.HasAdmin();
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
                return View(model);
            }
            try
            {
                await _registerLock.WaitAsync();

                if (await _userService.Exists(model.Username))
                {
                    SetErrorMessage("User already exists");
                    return View(model);
                }

                //Handle AdminToken
                bool createAsAdmin = false;
                if (model.AdminToken.HasValue)
                {
                    if (_settingsService.TryGetSettingRaw(SettingsService.KnownSettings.AdminToken, out string? token) && token != null)
                    {
                        if (Guid.TryParse(token, out var parsed) && parsed == model.AdminToken.Value)
                        {
                            createAsAdmin = true;
                        }
                        else
                        {
                            SetErrorMessage("Supplied token does not match");
                            return View(model);
                        }
                    }
                    else
                    {
                        SetErrorMessage("Token does not exist");
                        return View(model);
                    }
                }
                if (!model.HasAdmin && !createAsAdmin)
                {
                    SetErrorMessage("Regular users can only be created after the first administrator has been created");
                    return View(model);
                }

                if (await _userService.Create(model.Username, model.Password1))
                {
                    if (createAsAdmin)
                    {
                        await _userService.SetFlags(model.Username, UserFlags.Admin);
                        _settingsService.Delete(SettingsService.KnownSettings.AdminToken);
                        ViewData["HasAdmin"] = true;
                        model.HasAdmin = true;
                    }
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
            finally
            {
                _registerLock.Release();
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!IsLoggedIn)
            {
                return RedirectToLogin();
            }
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!IsLoggedIn)
            {
                return RedirectToLogin();
            }
            try
            {
                model.Validate();
                var rating = _passwordCheckerService.RatePassword(model.NewPassword1, false);
                if (!rating.IsSafe)
                {
                    throw new Exception($"The password is not safe. Make sure it's at least {rating.MinLength} characters long and contains at least {rating.MinScore} items of the following list: lowercase, uppercase, digits, symbols");
                }
                await _userService.ChangePassword(UserName!, model.OldPassword, model.NewPassword1);
                SetSuccessMessage("Password changed");
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
            await _userService.Ping(verify.User.Username);
            await HttpContext.SignInAsync(_userService.GetPrincipal(verify.User));
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
