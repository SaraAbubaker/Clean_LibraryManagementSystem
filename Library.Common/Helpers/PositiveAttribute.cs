
using System.ComponentModel.DataAnnotations;
using Library.Common.Exceptions;

namespace Library.Common.Helpers
{
    // Attribute for DTO/model validation
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PositiveAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (value is int intValue && intValue <= 0)
            {
                var msg = string.IsNullOrEmpty(ErrorMessage)
                    ? $"{validationContext.DisplayName} must be greater than zero."
                    : ErrorMessage;

                return new ValidationResult(msg);
            }

            return ValidationResult.Success;
        }
    }

    //for service layer
    public static class ValidationHelpers
    {
        public static void ValidatePositive(int value, string paramName)
        {
            if (value <= 0)
                throw new BadRequestException($"{paramName} must be greater than zero.");
        }
    }
}