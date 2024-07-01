using RomsBrowse.Data.Enums;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class AccountViewModel
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public DateTime LastActivity { get; set; }

        public UserFlags Flags { get; set; }

        public bool CanDelete { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsLocked { get; set; }

        public AccountViewModel(User u)
        {
            Id = u.Id;
            Username = u.Username;
            LastActivity = u.LastActivity;
            Flags = u.Flags;
            CanDelete = u.CanDelete;
            IsAdmin = u.IsAdmin;
            IsLocked = u.Flags.HasFlag(UserFlags.Locked);
        }
    }
}
