using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(UserId), nameof(RomFileId), IsUnique = true)]
    [Index(nameof(Created))]
    [PrimaryKey(nameof(UserId), nameof(RomFileId))]
    public class SRAM : IValidateable
    {
        [Required]
        public User User { get; set; }

        public int UserId { get; set; }

        [Required]
        public RomFile RomFile { get; set; }

        public int RomFileId { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required, MaxLength(1024 * 1024)]
        public byte[] Image { get; set; }

        [Required, MaxLength(1024 * 1024)]
        public byte[] Data { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
