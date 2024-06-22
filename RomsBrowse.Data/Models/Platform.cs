using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(ShortName), IsUnique = true)]
    [Index(nameof(Folder), IsUnique = true)]
    public class Platform : IValidateable
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string DisplayName { get; set; }

        [Required, StringLength(20)]
        public string ShortName { get; set; }

        [Required, StringLength(20)]
        public string EmulatorType { get; set; }

        [Required, StringLength(50)]
        public string Folder { get; set; }

        public virtual ICollection<RomFile> RomFiles { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
