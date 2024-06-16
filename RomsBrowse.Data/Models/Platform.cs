using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(ShortName), IsUnique = true)]
    [Index(nameof(Folder), IsUnique = true)]
    public class Platform
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string DisplayName { get; set; }

        [Required, StringLength(20)]
        public string ShortName { get; set; }

        [Required, StringLength(20)]
        public string EmulatorType { get; set; }

        [Required]
        public string Folder { get; set; }

        public virtual ICollection<RomFile> Roms { get; set; }
    }
#nullable restore
}
