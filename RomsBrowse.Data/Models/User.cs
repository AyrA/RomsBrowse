using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using RomsBrowse.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(LastActivity))]
    public class User : IValidateable
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string Hash { get; set; }

        public DateTime LastActivity { get; set; }

        [ValidEnum]
        public UserFlags Flags { get; set; }

        [NotMapped]
        public bool CanDelete => !Flags.HasFlag(UserFlags.Admin) && !Flags.HasFlag(UserFlags.NoExpireUser);

        [NotMapped]
        public bool IsAdmin
        {
            get
            {
                return Flags.HasFlag(UserFlags.Admin);
            }
            set
            {
                if (value)
                {
                    Flags |= UserFlags.Admin;
                }
                else
                {
                    Flags &= ~UserFlags.Admin;
                }
            }
        }

        [NotMapped]
        public bool CanExpire
        {
            get
            {
                return Flags.HasFlag(UserFlags.NoExpireUser);
            }
            set
            {
                if (value)
                {
                    Flags |= UserFlags.NoExpireUser;
                }
                else
                {
                    Flags &= ~UserFlags.NoExpireUser;
                }
            }
        }

        [NotMapped]
        public bool CanExpireSaves
        {
            get
            {
                return Flags.HasFlag(UserFlags.NoExpireSaveState);
            }
            set
            {
                if (value)
                {
                    Flags |= UserFlags.NoExpireSaveState;
                }
                else
                {
                    Flags &= ~UserFlags.NoExpireSaveState;
                }
            }
        }

        [NotMapped]
        public bool IsLocked
        {
            get
            {
                return Flags.HasFlag(UserFlags.Locked);
            }
            set
            {
                if (value)
                {
                    Flags |= UserFlags.Locked;
                }
                else
                {
                    Flags &= ~UserFlags.Locked;
                }
            }
        }

        [NotMapped]
        public bool CanSignIn => IsAdmin || !Flags.HasFlag(UserFlags.Locked);

        public virtual ICollection<SaveData> SaveData { get; set; }

        public void ResetExpiration() => LastActivity = DateTime.UtcNow;

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
