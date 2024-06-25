namespace RomsBrowse.Web.ViewModels
{
    public record FolderBrowseResultModel(string? CurrentFolder, string? ParentFolder, IEnumerable<FolderViewModel> Folders);
}
