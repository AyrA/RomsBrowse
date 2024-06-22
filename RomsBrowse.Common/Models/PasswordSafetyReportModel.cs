using System.Text.RegularExpressions;

namespace RomsBrowse.Common.Models
{
    public partial class PasswordSafetyReportModel
    {
        public bool IsSafe => Score >= MinScore && Length >= MinLength;

        public string? Password { get; }

        public bool HasLowerAlpha { get; }

        public bool HasUpperAlpha { get; }

        public bool HasDigit { get; }

        public bool HasSymbol { get; }

        public int Score { get; }

        public int MinScore { get; }

        public int MaxScore { get; }

        public int Length { get; }

        public int MinLength { get; }

        public int MaxLength { get; }

        public PasswordSafetyReportModel(string? password, bool exposePassword)
        {
            MaxScore = 4;
            MinScore = 3;
            MinLength = 8;
            MaxLength = ushort.MaxValue;

            if (exposePassword)
            {
                Password = password;
            }
            Length = password?.Length ?? 0;
            if (password != null)
            {
                HasLowerAlpha = LowerAlphaFinder().IsMatch(password);
                HasUpperAlpha = UpperAlphaFinder().IsMatch(password);
                HasDigit = DigitFinder().IsMatch(password);
                HasSymbol = SymbolFinder().IsMatch(password);
                Score =
                    (HasLowerAlpha ? 1 : 0) +
                    (HasUpperAlpha ? 1 : 0) +
                    (HasDigit ? 1 : 0) +
                    (HasSymbol ? 1 : 0);
            }
        }

        [GeneratedRegex("[a-z]")]
        private static partial Regex LowerAlphaFinder();

        [GeneratedRegex("[A-Z]")]
        private static partial Regex UpperAlphaFinder();

        [GeneratedRegex(@"\d")]
        private static partial Regex DigitFinder();

        [GeneratedRegex(@"[^A-Za-z\d]")]
        private static partial Regex SymbolFinder();
    }
}
