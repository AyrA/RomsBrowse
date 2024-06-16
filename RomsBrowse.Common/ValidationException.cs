namespace RomsBrowse.Common
{
    public class ValidationException(string fieldName, string? message, Exception? innerException) : Exception(message, innerException)
    {
        public string FieldName { get; } = fieldName;

        public ValidationException(string fieldName) : this(fieldName, $"Validation of field or property '{fieldName}' failed with an unspecified message")
        {
        }

        public ValidationException(string fieldName, string? message) : this(fieldName, message, null)
        {
        }
    }
}
