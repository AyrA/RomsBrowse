using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ViewModels
{
    public class SaveStateModel : IValidateable
    {
        public int GameId { get; set; }

        [Required]
        public IFormFile? Screenshot { get; set; }

        [Required]
        public IFormFile? SaveState { get; set; }

        [MemberNotNull(nameof(SaveState), nameof(Screenshot))]
        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
            if (Screenshot!.Length == 0)
            {
                throw new Common.Validation.ValidationException(nameof(Screenshot), "Screenshot cannot be empty");
            }
            if (SaveState!.Length == 0)
            {
                throw new Common.Validation.ValidationException(nameof(SaveState), "SaveState cannot be empty");
            }
        }
    }
}
