using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Common;
using RomsBrowse.Data.Enums;
using RomsBrowse.Web.Extensions;
using RomsBrowse.Web.ServiceModels;
using RomsBrowse.Web.Services;
using RomsBrowse.Web.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.Controllers
{
    [Authorize(Roles = nameof(UserFlags.Admin))]
    public class SettingsController(RomGatherService rgs, UserService userService, SettingsService ss) : BaseController(userService)
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new SettingsViewModel();
            vm.SetFromService(ss);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Index(SettingsViewModel model)
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
                ss.AddOrUpdate(SettingsService.KnownSettings.RomDirectory, model.RomsDirectory);
            }
            ss.AddOrUpdate(SettingsService.KnownSettings.AllowRegister, model.AllowRegister);
            ss.AddOrUpdate(SettingsService.KnownSettings.AnonymousPlay, model.AllowAnonymousPlay);
            ss.AddOrUpdate(SettingsService.KnownSettings.MaxSaveStatesPerUser, model.MaxSavesPerUser);
            ss.AddOrUpdate(SettingsService.KnownSettings.SaveStateExpiration, TimeSpan.FromDays(model.SaveStateExpiryDays));
            ss.AddOrUpdate(SettingsService.KnownSettings.UserExpiration, TimeSpan.FromDays(model.AccountExpiryDays));
            SetSuccessMessage("Settings updated. If you changed the Roms directory, perform a rescan");
            return View(model);
        }

        [HttpGet]
        public IActionResult Actions() => View(new ActionViewModel(rgs.IsScanning));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActionsAsync(ActionRequestModel model)
        {
            if (string.IsNullOrEmpty(model?.Action))
            {
                return View();
            }
            switch (model.Action)
            {
                case "Rescan":
                    if (!rgs.IsScanning)
                    {
                        rgs.Scan();
                        SetSuccessMessage("A rescan has been initiated. Depending on the number of files that need to be indexed, it may run for a while");
                    }
                    else
                    {
                        SetErrorMessage("A scan is already ongoing, please try again later");
                    }
                    break;
                case "Reset":
                    if (!rgs.IsScanning)
                    {
                        await rgs.Reset();
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
            return View(new ActionViewModel(rgs.IsScanning));
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
