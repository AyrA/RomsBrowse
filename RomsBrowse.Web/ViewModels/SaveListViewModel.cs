namespace RomsBrowse.Web.ViewModels
{
    public class SaveListViewModel
    {
        public int DeleteDaysBack { get; set; }

        public int MaxSaves { get; set; }

        public List<SaveDataViewModel> SaveStates { get; } = [];

        public List<SaveDataViewModel> SRAMs { get; } = [];
    }
}
