namespace RomsBrowse.Common
{
    public interface IValidateable
    {
        /// <summary>
        /// Validates the current instance,
        /// and throws <see cref="ValidationException"/> if validation fails
        /// </summary>
        void Validate();
    }
}
