namespace RomsBrowse.Web.ViewModels
{
    public class UserViewModel(string? username)
    {
        public bool IsLoggedIn => UserName != null;

        public string? UserName { get; } = username;
    }
}
