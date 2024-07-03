using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using RomsBrowse.Data.Enums;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.Extensions;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Web.ViewModels
{
    public class AccountViewModel : IValidateable, ISensitiveData
    {
        private UserFlags flags;

        public int Id { get; set; }

        [Required, ValidUsername]
        public string Username { get; set; }

        [SafePassword]
        public string? NewPassword1 { get; set; }

        public string? NewPassword2 { get; set; }

        public DateTime LastActivity { get; set; }

        [ValidEnum]
        public UserFlags Flags
        {
            get => flags;
            set => flags = value;
        }

        public bool IgnoreExpire
        {
            get
            {
                return flags.HasFlag(UserFlags.NoExpireUser) || IsAdmin;
            }
            set
            {
                flags.SetOrResetFlag(UserFlags.NoExpireUser, value);
            }
        }

        public bool IgnoreExpireSaves
        {
            get
            {
                return flags.HasFlag(UserFlags.NoExpireSaveState) || IsAdmin;
            }
            set
            {
                flags.SetOrResetFlag(UserFlags.NoExpireSaveState, value);
            }
        }

        public bool IsAdmin
        {
            get
            {
                return flags.HasFlag(UserFlags.Admin);
            }
            set
            {
                flags.SetOrResetFlag(UserFlags.Admin, value);
            }
        }

        public bool IsLocked
        {
            get
            {
                return flags.HasFlag(UserFlags.Locked) && !IsAdmin;
            }
            set
            {
                flags.SetOrResetFlag(UserFlags.Locked, value);
            }
        }

        public AccountViewModel()
        {
            Id = 0;
            Username = string.Empty;
            LastActivity = DateTime.UtcNow;
            Flags = UserFlags.Normal;
        }

        public AccountViewModel(User u)
        {
            Id = u.Id;
            Username = u.Username;
            LastActivity = u.LastActivity;
            Flags = u.Flags;
        }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
            if (Id < 1)
            {
                if (string.IsNullOrEmpty(NewPassword1))
                {
                    throw new Common.Validation.ValidationException(nameof(NewPassword1), "Password cannot be empty");
                }
                if (string.IsNullOrEmpty(NewPassword2))
                {
                    throw new Common.Validation.ValidationException(nameof(NewPassword2), "Password confirmation cannot be empty");
                }
            }
            if (!string.IsNullOrEmpty(NewPassword1) && !string.IsNullOrEmpty(NewPassword2))
            {
                if (NewPassword1 != NewPassword2)
                {
                    throw new Common.Validation.ValidationException(nameof(NewPassword2), "Passwords do not match");
                }
            }
        }

        public void ClearSensitiveData()
        {
            NewPassword1 = NewPassword2 = null;
        }
    }
}
