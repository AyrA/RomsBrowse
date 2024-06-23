using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomsBrowse.Data.Models
{
#nullable disable
    public class Settings : IValidateable
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None), StringLength(20)]
        public string Name { get; set; }

        public string? Value { get; set; }

        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
        }
    }
#nullable restore
}
