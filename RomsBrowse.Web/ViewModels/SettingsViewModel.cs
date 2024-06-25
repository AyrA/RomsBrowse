using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using RomsBrowse.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Web.ViewModels
{
    public class SettingsViewModel : IValidateable
    {
        [Required]
        public string? RomsDirectory { get; set; }

        public bool AllowAnonymousPlay { get; set; }

        public bool AllowRegister { get; set; }

        [Range(0, 9999)]
        public int MaxSavesPerUser { get; set; }

        [Range(0, 9999)]
        public int SaveStateExpiryDays { get; set; }

        [Range(0, 9999)]
        public int AccountExpiryDays { get; set; }

        public void SetFromService(SettingsService ss)
        {
            RomsDirectory = ss.GetValue<string>(SettingsService.KnownSettings.RomDirectory);
            AllowAnonymousPlay = ss.GetValue<bool>(SettingsService.KnownSettings.AnonymousPlay);
            AllowRegister = ss.GetValue<bool>(SettingsService.KnownSettings.AllowRegister);
            MaxSavesPerUser = ss.GetValue<int>(SettingsService.KnownSettings.MaxSaveStatesPerUser);
            SaveStateExpiryDays = (int)ss.GetValue<TimeSpan>(SettingsService.KnownSettings.SaveStateExpiration).TotalDays;
            AccountExpiryDays = (int)ss.GetValue<TimeSpan>(SettingsService.KnownSettings.UserExpiration).TotalDays;
        }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
            if (!Directory.Exists(RomsDirectory))
            {
                throw new Common.ValidationException(nameof(RomsDirectory), "Directory does not exist");
            }
        }
    }
}
