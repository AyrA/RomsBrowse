namespace RomsBrowse.Web.Extensions
{
    public static class EnumExtensions
    {
        public static T SetOrResetFlag<T>(this ref T enumValue, T flagValue, bool set) where T : struct, Enum
        {
            dynamic v = enumValue;
            dynamic f = flagValue;
            if (set)
            {
                v |= f;
            }
            else
            {
                v &= ~f;
            }
            enumValue = v;
            return enumValue;
        }
    }
}
