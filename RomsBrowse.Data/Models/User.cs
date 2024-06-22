using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using RomsBrowse.Data.Enums;
using System.ComponentModel.DataAnnotations;

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

        public UserFlags Flags { get; set; }

        public virtual ICollection<SaveState> SaveStates { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
