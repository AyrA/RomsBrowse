namespace RomsBrowse.Data.Enums
{
    [Flags]
    public enum UserFlags
    {
        Normal = 0,
        Locked = 1,
        Admin = Locked << 1
    }
}
