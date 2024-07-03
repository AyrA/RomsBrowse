using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ViewModels
{
    public class RegisterViewModel : IValidateable, ISensitiveData
    {
        [Required, ValidUsername]
        public string? Username { get; set; }

        [Required, SafePassword]
        public string? Password1 { get; set; }

        [Required]
        public string? Password2 { get; set; }

        public Guid? AdminToken { get; set; }

        public bool UserCreated { get; set; }

        public bool HasAdmin { get; set; }

        public void ClearSensitiveData()
        {
            AdminToken = null;
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
                throw new Common.Validation.ValidationException(nameof(Password2), "Passwords do not match");
            }
        }
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
    }
}
