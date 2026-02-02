using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.BorrowRecord
{
    public class RequestBorrowDto
    {
        [Required]
        [Positive(ErrorMessage = "BookId must be a positive number.")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [CustomValidation(typeof(RequestBorrowDto), nameof(ValidateDueDate))]
        public DateOnly? DueDate { get; set; }



        // Validate DueDate is not before today
        public static ValidationResult? ValidateDueDate(DateOnly? dueDate, ValidationContext context)
        {
            if (dueDate == null) return new ValidationResult("Due date is required.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dueDate < today)
                return new ValidationResult("Due date cannot be in the past.");

            return ValidationResult.Success;
        }
    }
}
