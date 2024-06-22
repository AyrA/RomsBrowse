using AyrA.AutoDI;
using RomsBrowse.Common.Models;

namespace RomsBrowse.Common.Services
{
    [AutoDIRegister(AutoDIType.Transient, typeof(IPasswordCheckerService))]
    public class PasswordCheckerService : IPasswordCheckerService
    {
        public PasswordSafetyReportModel RatePassword(string? password, bool expose)
        {
            return new(password, expose);
        }

        public bool IsSafePassword(string? password) => RatePassword(password, false).IsSafe;

        public void EnsureSafePassword(string? password)
        {
            if (!IsSafePassword(password))
            {
                throw new ArgumentException("Password is not safe");
            }
        }
    }
}
