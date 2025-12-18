using Library.Shared.Exceptions;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Library.Shared.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PositiveAttribute : Attribute
    {
    }

    public static class Validate
    {
        public static void NotNull(object? value, string name)
        {
            if (value == null)
                throw new BadRequestException($"{name} is required.");
        }

        public static void NotEmpty(string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException($"{name} is required.");
        }

        public static void Positive(int value, string name)
        {
            if (value <= 0)
                throw new BadRequestException($"{name} must be greater than zero.");
        }

        public static T Exists<T>(T? entity, int id)
        {
            if (entity == null)
                throw new NotFoundException($"{typeof(T).Name} with id {id} is not found");
            return entity;
        }


        public static void ValidateModel(object model)
        {
            if (model == null) throw new BadRequestException("Model cannot be null.");

            var errors = new List<string>();
            var props = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var value = prop.GetValue(model);

                //Required check
                if (Attribute.IsDefined(prop, typeof(RequiredAttribute)))
                {
                    if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                        errors.Add($"{prop.Name} is required.");
                }

                //String Length check
                var strLen = prop.GetCustomAttribute<StringLengthAttribute>();
                if (strLen != null && value is string strValue)
                {
                    if (strValue.Length > strLen.MaximumLength)
                        errors.Add($"{prop.Name} cannot be longer than {strLen.MaximumLength} characters.");
                    if (strValue.Length < strLen.MinimumLength)
                        errors.Add($"{prop.Name} must be at least {strLen.MinimumLength} characters.");
                }

                //MinLength / MaxLength
                var minLenAttr = prop.GetCustomAttribute<MinLengthAttribute>();
                if (minLenAttr != null)
                {
                    if (value is string sMin && sMin.Length < minLenAttr.Length)
                        errors.Add($"{prop.Name} must be at least {minLenAttr.Length} characters.");

                    else if (value is ICollection cMin && cMin.Count < minLenAttr.Length)
                        errors.Add($"{prop.Name} must contain at least {minLenAttr.Length} items.");
                }

                var maxLenAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLenAttr != null)
                {
                    if (value is string sMax && sMax.Length > maxLenAttr.Length)
                        errors.Add($"{prop.Name} cannot be longer than {maxLenAttr.Length} characters.");

                    else if (value is ICollection cMax && cMax.Count > maxLenAttr.Length)
                        errors.Add($"{prop.Name} cannot contain more than {maxLenAttr.Length} items.");
                }

                //Range
                var rangeAttr = prop.GetCustomAttribute<RangeAttribute>();
                if (rangeAttr != null && value != null)
                {
                    try
                    {
                        var numeric = Convert.ToDouble(value);
                        var min = Convert.ToDouble(rangeAttr.Minimum);
                        var max = Convert.ToDouble(rangeAttr.Maximum);
                        if (numeric < min || numeric > max)
                            errors.Add($"{prop.Name} must be between {min} and {max}.");
                    }
                    catch
                    {
                        errors.Add($"{prop.Name}: Range attribute applied but value is not numeric.");
                    }
                }

                //EmailAddress
                var emailAttr = prop.GetCustomAttribute<EmailAddressAttribute>();
                if (emailAttr != null && value is string emailStr && !string.IsNullOrWhiteSpace(emailStr))
                {
                    var attr = new EmailAddressAttribute();
                    if (!attr.IsValid(emailStr))
                        errors.Add($"{prop.Name} is not a valid email address.");
                }


                //Positive int (custom attribute)
                if (Attribute.IsDefined(prop, typeof(PositiveAttribute)) && value is int intValue && intValue <= 0)
                {
                    errors.Add($"{prop.Name} must be greater than zero.");
                }
            }

            if (errors.Any())
                throw new BadRequestException(string.Join(Environment.NewLine, errors));
        }
    }
}
