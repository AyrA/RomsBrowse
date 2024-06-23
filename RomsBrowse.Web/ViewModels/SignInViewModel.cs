using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ViewModels
{
    public class SignInViewModel : IValidateable, ISensitiveData
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        public string? RedirectUrl { get; set; }

        public void ClearSensitiveData()
        {
            Password = null;
        }

        [MemberNotNull(nameof(Username), nameof(Password))]
        public void Validate()
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        {
            ValidationTools.ValidatePublic(this);
        }
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
    }
}
