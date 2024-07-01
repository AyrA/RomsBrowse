namespace RomsBrowse.Web.ViewModels
{
    public class AccountsViewModel
    {
        public PagingViewModel Paging { get; set; } = new(1, 1, 0);

        public AccountViewModel[] Accounts { get; set; } = [];
    }
}
