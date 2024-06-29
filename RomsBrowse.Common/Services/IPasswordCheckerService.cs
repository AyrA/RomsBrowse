using RomsBrowse.Common.Models;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Common.Services
{
    public interface IPasswordCheckerService
    {
        void EnsureSafePassword([NotNull] string? password);
        bool IsSafePassword([NotNullWhen(true)] string? password);
        PasswordSafetyReportModel RatePassword(string? password, bool expose);
    }
}