using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(FileName))]
    public class RomFile : IValidateable
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string FileName { get; set; }

        [Required, StringLength(255)]
        public string DisplayName { get; set; }

        [Required, StringLength(255)]
        public string FilePath { get; set; }

        [Range(0, int.MaxValue)]
        public int Size { get; set; }

        [Required, MaxLength(32), MinLength(32)]
        public byte[] Sha256 { get; set; }

        [Required]
        public Platform Platform { get; set; }

        public int PlatformId { get; set; }

        public virtual ICollection<SaveState> SaveStates { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
