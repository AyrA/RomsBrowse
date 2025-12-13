namespace RomsBrowse.Data.Enums;

[Flags]
public enum SaveFlags
{
    /// <summary>
    /// Emulator save state
    /// </summary>
    State = 1,
    /// <summary>
    /// ROM save functionality (cartridge SRAM)
    /// </summary>
    SRAM = 2
}
