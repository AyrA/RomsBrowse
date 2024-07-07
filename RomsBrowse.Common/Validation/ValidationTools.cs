using RomsBrowse.Common.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RomsBrowse.Common.Validation
{
    public static class ValidationTools
    {
        private const BindingFlags AllFields = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public static void ValidateFields(object owner, params string[] fieldNames)
        {
            var t = owner.GetType();
            var publicProps = t.GetProperties(AllFields);
            var publicFields = t.GetFields(AllFields);

            foreach (var name in fieldNames)
            {
                var prop = publicProps.FirstOrDefault(m => m.Name == name);
                var field = publicFields.FirstOrDefault(m => m.Name == name);
                if (prop != null)
                {
                    ValidateField(prop, owner);
                    return;
                }
                if (field != null)
                {
                    ValidateField(field, owner);
                    return;
                }
                throw new ArgumentException($"Property or field not found: {name}", nameof(fieldNames));
            }
        }

        public static void ValidateField(PropertyInfo prop, object owner)
        {
            Validate(prop.GetValue(owner), prop.GetCustomAttributes(), prop.Name);
        }

        public static void ValidateField(FieldInfo field, object owner)
        {
            Validate(field.GetValue(owner), field.GetCustomAttributes(), field.Name);
        }

        public static void ValidatePublic(object instance)
        {
            foreach (var prop in instance.GetType().GetProperties())
            {
                if (prop.CanWrite)
                {
                    ValidateField(prop, instance);
                }
            }
            foreach (var field in instance.GetType().GetFields())
            {
                if (!field.IsInitOnly)
                {
                    ValidateField(field, instance);
                }
            }
        }

        public static void ValidateRange(IComparable num, IComparable min, IComparable max, [CallerArgumentExpression(nameof(num))] string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(num);
            ArgumentNullException.ThrowIfNull(min);
            ArgumentNullException.ThrowIfNull(max);
            ArgumentNullException.ThrowIfNull(paramName);

            if (num.CompareTo(min) < 0)
            {
                throw new ValidationException(paramName, $"Value {num} is too small. Must be at least {min}");
            }
            if (num.CompareTo(max) > 0)
            {
                throw new ValidationException(paramName, $"Value {num} is too big. Must be at most {min}");
            }
        }

        private static void Validate(object? value, IEnumerable<Attribute> attributes, string propName)
        {
            foreach (var attr in attributes)
            {
                var at = attr.GetType();
                if (attr is RequiredAttribute req)
                {
                    if (!req.IsValid(value))
                    {
                        throw new ValidationException(propName, $"Property or field {propName} cannot be null");
                    }
                }
                else if (attr is RangeAttribute ra)
                {
                    try
                    {
                        ra.Validate(value, propName);
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new ValidationException(propName, $"Unable to validate property or field {propName} due to an internal error. {ex.Message} See inner exception for details", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new ValidationException(propName, $"The value '{value}' of property or field {propName} is outside of the permitted range {ra.Minimum} to {ra.Maximum}", ex);
                    }
                }
                else if (attr is MinLengthAttribute minL)
                {
                    try
                    {
                        minL.Validate(value, propName);
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new ValidationException(propName, $"Unable to validate property or field {propName} due to an internal error. {ex.Message} See inner exception for details", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new ValidationException(propName, $"The value '{value}' of property or field {propName} is too small. Must be at least {minL.Length}", ex);
                    }
                }
                else if (attr is MaxLengthAttribute maxL)
                {
                    try
                    {
                        maxL.Validate(value, propName);
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new ValidationException(propName, $"Unable to validate property or field {propName} due to an internal error. {ex.Message} See inner exception for details", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new ValidationException(propName, $"The value '{value}' of property or field {propName} is too big. Must be at most {maxL.Length}", ex);
                    }
                }
                else if (attr is StringLengthAttribute sl)
                {
                    if (value is string s)
                    {
                        if (s.Length < sl.MinimumLength)
                        {
                            throw new ValidationException(propName, $"{propName} is too small. Must be at least {sl.MinimumLength} but is {s.Length}");
                        }
                        if (s.Length > sl.MaximumLength)
                        {
                            throw new ValidationException(propName, $"{propName} is too big. Must be at most {sl.MaximumLength} but is {s.Length}");
                        }
                    }
                    else
                    {
                        throw new ValidationException(propName, $"Property {propName} has {nameof(StringLengthAttribute)} but is not a string. Is: {value?.GetType()}");
                    }
                }
                else if (attr is SafePasswordAttribute)
                {
                    if (value != null)
                    {
                        if (value is string s)
                        {
                            if (!new PasswordCheckerService().IsSafePassword(s))
                            {
                                throw new ValidationException(propName, "Password is not safe");
                            }
                        }
                        else
                        {
                            throw new ValidationException(propName, $"Property {propName} has {nameof(SafePasswordAttribute)} but is not a string. Is: {value?.GetType()}");
                        }
                    }
                }
                else if (attr is ValidUsernameAttribute)
                {
                    if (value != null)
                    {
                        if (value is string s)
                        {
                            if (!ValidUsernameAttribute.IsValidUsername(s))
                            {
                                throw new ValidationException(propName,
                                    $"Username is not valid. Must be {ValidUsernameAttribute.RegexDescription}");
                            }
                        }
                        else
                        {
                            throw new ValidationException(propName, $"Property {propName} has {nameof(ValidUsernameAttribute)} but is not a string. Is: {value?.GetType()}");
                        }
                    }
                }
                else if (attr is ValidEnumAttribute ea)
                {
                    var t = (value?.GetType())
                        ?? throw new ValidationException(propName, $"Property {propName} has {nameof(ValidEnumAttribute)} but is null");

                    if (!t.IsEnum)
                    {
                        throw new ValidationException(propName, $"Property {propName} has {nameof(ValidEnumAttribute)} but is not an enum type");
                    }
                    if (Enum.GetUnderlyingType(t) != typeof(int))
                    {
                        throw new InvalidOperationException("Enum check currently only works for enum values with a 32 bit signed integer underlying type");
                    }

                    int allFlags = 0;
                    int checkValue = Convert.ToInt32(value);
                    foreach (var v in Enum.GetValuesAsUnderlyingType(t))
                    {
                        allFlags |= Convert.ToInt32(v);
                    }

                    if (allFlags != (allFlags | checkValue))
                    {
                        throw new ValidationException(propName, $"Property {propName} has invalid enum flags value");
                    }
                }
            }
        }
    }
}
