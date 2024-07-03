using System.Text.RegularExpressions;

namespace RomsBrowse.Common.Validation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public partial class ValidUsernameAttribute : Attribute
    {
        private const string usernameRegexValue = @"^[a-zA-Z\d]+$";

        public static readonly string UsernameRegex = usernameRegexValue;
        public static readonly string RegexDescription = "alphanumeric";

        public static bool IsValidUsername(string? value)
        {
            return !string.IsNullOrEmpty(value)
                && UsernameValidator().IsMatch(value);
        }

        [GeneratedRegex(usernameRegexValue)]
        private static partial Regex UsernameValidator();
    }


}
