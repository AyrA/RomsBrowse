using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using System.ComponentModel.DataAnnotations;
using VE = RomsBrowse.Common.Validation.ValidationException;

namespace RomsBrowse.Web.ViewModels
{
    public class DbSetupViewModel : IValidateable, ISensitiveData
    {
        [Required]
        public string? ServerInstance { get; set; }

        [Required]
        public string? DatabaseName { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool UseWindowsAuth { get; set; }

        public bool Encrypt { get; set; }

        public string CurrentUser { get; } =
            OperatingSystem.IsWindows()
            ? string.Join('\\', Environment.UserDomainName, Environment.UserName)
            : Environment.UserName;

        public void ClearSensitiveData()
        {
            Password = null;
        }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
            if (UseWindowsAuth)
            {
                if (!string.IsNullOrWhiteSpace(Username))
                {
                    throw new VE(nameof(Username), "Cannot use windows authentication and use a username at the same time");
                }
            }
            else if (string.IsNullOrWhiteSpace(Username))
            {
                throw new VE(nameof(Username), "Username is required if windows authentication is disabled");
            }
            else if (string.IsNullOrWhiteSpace(Password))
            {
                throw new VE(nameof(Password), "If a username is specified, a password is required");
            }
        }
    }
}
