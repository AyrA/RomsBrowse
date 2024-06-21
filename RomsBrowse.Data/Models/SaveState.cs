using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(Owner), nameof(Game), IsUnique = true)]
    [Index(nameof(Created))]
    public class SaveState
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public User Owner { get; set; }

        [Required]
        public RomFile Game { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required, MaxLength(1024 * 1024)]
        public byte[] Image { get; set; }

        [Required, MaxLength(1024 * 1024)]
        public byte[] Data { get; set; }
    }
#nullable restore
}
