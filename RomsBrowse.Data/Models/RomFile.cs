using System.ComponentModel.DataAnnotations;

namespace RomsBrowse.Data.Models
{
#nullable disable
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
    }
#nullable restore
}
