using RomsBrowse.Common;
using RomsBrowse.Common.Interfaces;
using RomsBrowse.Web.ServiceModels;

namespace RomsBrowse.Web.ViewModels
{
    public class SaveOperationViewModel : IValidateable
    {
        public int Id { get; set; }

        public SaveType Type { get; set; }

        public void Validate()
        {
            if (Id < 0)
            {
                throw new ValidationException(nameof(Id), "Game id is invalid");
            }
            if (!Enum.IsDefined(Type) || Type == SaveType.None)
            {
                throw new ValidationException(nameof(Type), "Invalid save type");
            }
        }
    }
}
