using RomsBrowse.Common.Validation;

namespace RomsBrowse.Common.Interfaces;

/// <summary>
/// Represents a type that provides a generic validation function
/// </summary>
public interface IValidateable
{
    /// <summary>
    /// Validates the current instance,
    /// and throws <see cref="ValidationException"/> if validation fails
    /// </summary>
    void Validate();
}
