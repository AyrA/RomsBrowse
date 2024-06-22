using RomsBrowse.Common.Models;

namespace RomsBrowse.Common.Services
{
    public interface IPasswordCheckerService
    {
        void EnsureSafePassword(string? password);
        bool IsSafePassword(string? password);
        PasswordSafetyReportModel RatePassword(string? password, bool expose);
    }
}