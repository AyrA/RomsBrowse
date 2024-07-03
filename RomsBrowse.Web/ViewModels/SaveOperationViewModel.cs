using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using RomsBrowse.Data.Enums;

namespace RomsBrowse.Web.ViewModels
{
    public class SaveOperationViewModel : IValidateable
    {
        public int Id { get; set; }

        public SaveFlags Type { get; set; }

        public void Validate()
        {
            if (Id < 0)
            {
                throw new ValidationException(nameof(Id), "Game id is invalid");
            }
            if (Type != SaveFlags.State && Type != SaveFlags.SRAM)
            {
                throw new ValidationException(nameof(Type), "Invalid save type");
            }
        }
    }
}
