using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ViewModels
{
    public class SaveSRAMModel : IValidateable
    {
        public int GameId { get; set; }

        [Required]
        public IFormFile? SaveState { get; set; }

        [MemberNotNull(nameof(SaveState))]
        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
            if (SaveState!.Length == 0)
            {
                throw new Common.ValidationException(nameof(SaveState), "SaveState cannot be empty");
            }
        }
    }
}
