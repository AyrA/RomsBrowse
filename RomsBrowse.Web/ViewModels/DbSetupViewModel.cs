using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
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

        public string GetConnectionString()
        {
            Validate();
            var builder = new DbConnectionStringBuilder
            {
                { "Server", ServerInstance },
                { "Database", DatabaseName }
            };

            if (UseWindowsAuth)
            {
                builder.Add("Trusted_Connection", "True");
            }
            else
            {
                builder.Add($"User Id", Username!);
                builder.Add($"Password", Password!);
            }
            builder.Add("Encrypt", Encrypt ? "True" : "False");

            return builder.ToString();
        }

        public void ClearSensitiveData()
        {
            Password = null;
        }

#pragma warning disable CS8774
        [MemberNotNull(nameof(DatabaseName), nameof(ServerInstance))]
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
#pragma warning restore CS8774
    }
}
