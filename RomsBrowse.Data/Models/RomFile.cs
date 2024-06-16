using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
    [Index(nameof(FileName))]
    public class RomFile
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FileName { get; set; }

        [Required, StringLength(255)]
        public string FilePath { get; set; }

        public int Size { get; set; }

        [MaxLength(32)]
        public byte[] Sha256 { get; set; }

        public Platform Platform { get; set; }

        public int PlatformId { get; set; }
    }
#nullable restore
}
