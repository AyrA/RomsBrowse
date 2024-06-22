using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ViewModels
{
    public class RegisterViewModel : IValidateable, ISensitiveData
    {
        [Required]
        public string? Username { get; set; }

        [Required, SafePassword]
        public string? Password1 { get; set; }

        [Required]
        public string? Password2 { get; set; }

        public bool UserCreated { get; set; }

        public void ClearSensitiveData()
        {
            Password1 = null;
            Password2 = null;
        }

        [MemberNotNull(nameof(Username), nameof(Password1), nameof(Password2))]
        public void Validate()
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        {
            ValidationTools.ValidatePublic(this);
            if (Password1 != Password2)
            {
                throw new Common.ValidationException(nameof(Password2), "Passwords do not match");
            }
        }
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
    }
}
