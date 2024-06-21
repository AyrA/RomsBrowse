using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(Username), IsUnique = true)]
    public class User : IValidateable
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string Hash { get; set; }

        public DateTime LastLogin { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
