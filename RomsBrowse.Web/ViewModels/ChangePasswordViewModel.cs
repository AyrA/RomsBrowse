using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ViewModels
{
    public class ChangePasswordViewModel : IValidateable, ISensitiveData
    {
        [Required]
        public string? OldPassword { get; set; }

        [Required]
        public string? NewPassword1 { get; set; }

        [Required]
        public string? NewPassword2 { get; set; }

        public void ClearSensitiveData()
        {
            OldPassword = NewPassword1 = NewPassword2 = null;
        }

        [MemberNotNull(nameof(OldPassword), nameof(NewPassword1), nameof(NewPassword2))]
        public void Validate()
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        {
            ValidationTools.ValidatePublic(this);
            if (NewPassword1 != NewPassword2)
            {
                throw new Common.ValidationException(nameof(NewPassword2), "New passwords are not identical");
            }
            if (OldPassword == NewPassword1)
            {
                throw new Common.ValidationException(nameof(NewPassword1), "New password cannot be the same as the old password");
            }
        }
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
    }
}
