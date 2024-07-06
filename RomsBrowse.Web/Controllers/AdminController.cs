using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Common.Validation;
using RomsBrowse.Data.Enums;
using RomsBrowse.Data.Services;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.ServiceModels;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.Controllers
{
    [Authorize(Roles = nameof(UserFlags.Admin))]
    public class AdminController : BaseController
    {
        private readonly UserService _userService;
        private readonly RomGatherService _rgs;
        private readonly SettingsService _ss;
        private readonly ConnectionStringProvider _csp;

        public AdminController(RomGatherService rgs, UserService userService, SettingsService ss, ConnectionStringProvider csp) : base(userService)
        {
            _userService = userService;
            _rgs = rgs;
            _ss = ss;
            _csp = csp;
        }

        [HttpGet]
        public async Task<IActionResult> Accounts(int page, string? search)
        {
            var vm = await _userService.GetAccounts(page, 20, search);
            return View(vm);
        }

        [HttpGet]
        public IActionResult AccountEdit(int id)
        {
            if (CurrentUser!.Id == id)
            {
                SetRedirectMessage("Cannot edit your own user this way", false);
                return RedirectToAction(nameof(Accounts));
            }
            var user = _userService.Get(id, false);
            if (user == null)
            {
                return NotFound();
            }
            return View(new AccountViewModel(user));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountEdit(AccountViewModel model)
        {
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                SetRedirectMessage(ex);
                return RedirectToAction(nameof(AccountEdit), new { model.Id });
            }
            if (CurrentUser!.Id == model.Id)
            {
                SetRedirectMessage("Cannot edit your own user this way", false);
                return RedirectToAction(nameof(AccountEdit), new { model.Id });
            }
            var user = _userService.Get(model.Id, true);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                user.Username = model.Username;
                user.Flags = model.Flags;
                if (!string.IsNullOrEmpty(model.NewPassword1))
                {
                    _userService.SetNewPassword(user, model.NewPassword1);
                }
                await _userService.SaveChanges(user);
                SetRedirectMessage("User updated", true);
            }
            catch (Exception ex)
            {
                SetRedirectMessage(ex);
            }
            return RedirectToAction(nameof(AccountEdit), new { model.Id });
        }

        [HttpGet]
        public IActionResult AccountCreate()
        {
            return View(nameof(AccountEdit), new AccountViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountCreate(AccountViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.NewPassword1))
                {
                    throw new Exception("Password is required");
                }
                model.Validate();
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
                return View(nameof(AccountEdit), model);
            }
            var user = _userService.Get(model.Username, false);
            if (user != null)
            {
                SetErrorMessage("A user with this name already exists");
                return View(nameof(AccountEdit), model);
            }
            try
            {
                if (!await _userService.Create(model.Username, model.NewPassword1))
                {
                    throw new Exception("Failed to create user");
                }
                SetRedirectMessage("User created", true);
                return RedirectToAction(nameof(Accounts));
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
            }
            return View(nameof(AccountEdit), model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountAction(AccountActionRequestModel model)
        {
            if (string.IsNullOrEmpty(model?.Action) || model.UserId < 1)
            {
                return RedirectToAction(nameof(Accounts));
            }
            try
            {
                if (model.UserId == CurrentUser?.Id)
                {
                    throw new InvalidOperationException("Cannot edit or delete current user");
                }
                switch (model.Action)
                {
                    case "Delete":
                        await _userService.Delete(model.UserId);
                        SetRedirectMessage("User deleted", true);
                        break;
                    default:
                        throw new Exception("Invalid action");
                }
            }
            catch (Exception ex)
            {
                SetRedirectMessage(ex);
            }
            return RedirectToAction(nameof(Accounts));
        }

        [HttpGet]
        public IActionResult Settings()
        {
            var vm = new SettingsViewModel();
            vm.SetFromService(_ss);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Settings(SettingsViewModel model)
        {
            try
            {
                model.Validate();
            }
            catch (ValidationException ex)
            {
                SetErrorMessage(ex);
                return View(model);
            }
            if (IsValidConfigOrDir(model.RomsDirectory))
            {
                _ss.AddOrUpdate(SettingsService.KnownSettings.RomDirectory, model.RomsDirectory);
            }
            _ss.AddOrUpdate(SettingsService.KnownSettings.AllowRegister, model.AllowRegister);
            _ss.AddOrUpdate(SettingsService.KnownSettings.AnonymousPlay, model.AllowAnonymousPlay);
            _ss.AddOrUpdate(SettingsService.KnownSettings.MaxSaveStatesPerUser, model.MaxSavesPerUser);
            _ss.AddOrUpdate(SettingsService.KnownSettings.SaveStateExpiration, TimeSpan.FromDays(model.SaveStateExpiryDays));
            _ss.AddOrUpdate(SettingsService.KnownSettings.UserExpiration, TimeSpan.FromDays(model.AccountExpiryDays));
            SetSuccessMessage("Settings updated. If you changed the Roms directory, perform a rescan");
            return View(model);
        }

        [HttpGet]
        public IActionResult Actions() => View(new ActionViewModel(_rgs.IsScanning));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Actions(ActionRequestModel model)
        {
            if (string.IsNullOrEmpty(model?.Action))
            {
                return View();
            }
            switch (model.Action)
            {
                case "Init":
                    if (!_rgs.IsScanning)
                    {
                        _csp.ResetConnectionString();
                        SetSuccessMessage("Database connection settings have been deleted");
                    }
                    else
                    {
                        SetErrorMessage("A ROM scan is currently running, please try again later");
                    }
                    break;
                case "Rescan":
                    if (!_rgs.IsScanning)
                    {
                        _rgs.Scan();
                        SetSuccessMessage("A rescan has been initiated. Depending on the number of files that need to be indexed, it may run for a while");
                    }
                    else
                    {
                        SetErrorMessage("A scan is already ongoing, please try again later");
                    }
                    break;
                case "Reset":
                    if (!_rgs.IsScanning)
                    {
                        await _rgs.Reset();
                        SetSuccessMessage("A reset has been performed. Perform a rescan to reindex the rom files");
                    }
                    else
                    {
                        SetErrorMessage("A scan is already ongoing, please try again later");
                    }
                    break;
                default:
                    SetErrorMessage("Invalid action");
                    break;
            }
            return View(new ActionViewModel(_rgs.IsScanning));
        }

        public IActionResult Platforms() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Folder(FolderBrowseViewModel model)
        {
            var drives = DriveInfo.GetDrives().Where(m => m.IsReady).ToArray();
            if (string.IsNullOrEmpty(model.Folder))
            {
                var folders = drives.Select(m => new FolderViewModel($"{m.RootDirectory.FullName} ({m.VolumeLabel})", m.RootDirectory.FullName, false)).ToArray();
                var result = new FolderBrowseResultModel(null, null, folders);
                return Json(result);
            }

            string[] folderNames;
            try
            {
                folderNames = Directory.GetDirectories(model.Folder);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound("Directory not found");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                return new ObjectResult(ex.Message);
            }
            return Json(new FolderBrowseResultModel(model.Folder, Path.GetDirectoryName(model.Folder), ToModel(folderNames)));
        }

        private static IEnumerable<FolderViewModel> ToModel(IEnumerable<string> contents)
        {
            foreach (var content in contents)
            {
                bool hasJson;
                //Ensure directory exists
                try
                {
                    if (!Directory.Exists(content))
                    {
                        continue;
                    }
                }
                catch
                {
                    continue;
                }
                //Check if ROM config file exists
                try
                {
                    var config = Path.Combine(content, "config.json");
                    hasJson = System.IO.File.Exists(config);
                    if (hasJson)
                    {
                        if (!IsValidConfigOrDir(config))
                        {
                            hasJson = false;
                        }
                    }
                }
                catch
                {
                    hasJson = false;
                }
                yield return new FolderViewModel(Path.GetFileName(content), content, hasJson);
            }
        }

        private static bool IsValidConfigOrDir([NotNullWhen(true)] string? configFile)
        {
            if (string.IsNullOrEmpty(configFile))
            {
                return false;
            }
            if (System.IO.File.Exists(Path.Combine(configFile, "config.json")))
            {
                configFile = Path.Combine(configFile, "config.json");
            }
            try
            {
                var config = System.IO.File.ReadAllText(configFile)
                    .FromJson<RomDirConfig[]>();
                return config != null && config.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
