using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class UserViewModel(User? user)
    {
        public bool IsLoggedIn => user != null;

        public bool IsAdmin => user?.IsAdmin ?? false;

        public string? UserName { get; } = user?.Username;
    }
}
