using RomsBrowse.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Web.ServiceModels
{
    public class AccountVerifyModel(bool isValid, User? user)
    {
        [MemberNotNullWhen(true, nameof(User))]
        public bool IsValid => isValid && user != null;

        public User? User => user;
    }
}
