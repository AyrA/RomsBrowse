namespace RomsBrowse.Web.ViewModels
{
    public class SaveListViewModel
    {
        public int DeleteDaysBack { get; set; }
        public int MaxSaves { get; set; }
        public List<SaveStateViewModel> SaveStates { get; } = [];
        public List<SRAMViewModel> SRAMs { get; } = [];
    }
}
