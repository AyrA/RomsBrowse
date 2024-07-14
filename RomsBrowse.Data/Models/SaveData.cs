using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using RomsBrowse.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(UserId), nameof(RomFileId), nameof(Flags), IsUnique = true)]
    [Index(nameof(Created))]
    [PrimaryKey(nameof(UserId), nameof(RomFileId), nameof(Flags))]
    public class SaveData : IValidateable
    {
        [Required]
        public User User { get; set; }

        public int UserId { get; set; }

        [Required]
        public RomFile RomFile { get; set; }

        public int RomFileId { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [ValidEnum]
        public SaveFlags Flags { get; set; }

        [Required, MaxLength(1024 * 1024)]
        public byte[] Image { get; set; }

        [Required, MaxLength(1024 * 1024 * 32)]
        public byte[] Data { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
            if (Flags == 0)
            {
                throw new Common.Validation.ValidationException(nameof(Flags), "Flags are not set");
            }
        }
    }
#nullable restore
}
