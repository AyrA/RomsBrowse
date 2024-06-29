using AyrA.AutoDI;
using RomsBrowse.Common.Models;
using System.Diagnostics.CodeAnalysis;

namespace RomsBrowse.Common.Services
{
    [AutoDIRegister(AutoDIType.Transient, typeof(IPasswordCheckerService))]
    public class PasswordCheckerService : IPasswordCheckerService
    {
        public PasswordSafetyReportModel RatePassword(string? password, bool expose)
        {
            return new(password, expose);
        }

        public bool IsSafePassword([NotNullWhen(true)] string? password)
        {
            ArgumentNullException.ThrowIfNull(password);
            return RatePassword(password, false).IsSafe;
        }

        public void EnsureSafePassword([NotNull] string? password)
        {
            if (!IsSafePassword(password))
            {
                throw new ArgumentException("Password is not safe");
            }
        }
    }
}
