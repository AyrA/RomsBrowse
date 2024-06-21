namespace RomsBrowse.Web.ViewModels
{
    public class UserViewModel(Guid userId)
    {
        public bool IsLoggedIn => UserId != Guid.Empty;

        public Guid UserId { get; } = userId;
    }
}
