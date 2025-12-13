namespace RomsBrowse.Common.Interfaces;

/// <summary>
/// Represents a type that contains sensitive data
/// which needs to be able to be cleared
/// </summary>
public interface ISensitiveData
{
    /// <summary>
    /// Clear sensitive data
    /// </summary>
    void ClearSensitiveData();
}
