namespace RomsBrowse.Data.Enums
{
    [Flags]
    public enum UserFlags
    {
        /// <summary>
        /// Regular user without any other flags
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Account is locked
        /// </summary>
        Locked = 1,
        /// <summary>
        /// Account has admin rights
        /// </summary>
        /// <remarks>
        /// This usually overrides
        /// <see cref="Locked"/>,
        /// and implies <see cref="NoExpireUser"/>
        /// </remarks>
        Admin = Locked << 1,
        /// <summary>
        /// User account does not expire due to inactivity
        /// </summary>
        NoExpireUser = Admin << 1,
        /// <summary>
        /// Save states of given user account do not expire
        /// </summary>
        NoExpireSaveState = NoExpireUser << 1,
    }
}
