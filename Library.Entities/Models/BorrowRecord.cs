
using System.ComponentModel.DataAnnotations;
using Library.Entities.Base;

namespace Library.Entities.Models
{
    public class BorrowRecord : AuditBase
    {
        [Required(ErrorMessage = "Borrow date is required.")]
        [CustomValidation(typeof(BorrowRecord), nameof(ValidateDates))]
        public DateOnly BorrowDate { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [CustomValidation(typeof(BorrowRecord), nameof(ValidateDates))]
        public DateOnly DueDate { get; set; }

        [CustomValidation(typeof(BorrowRecord), nameof(ValidateReturnDate))]
        public DateOnly? ReturnDate { get; set; } //nullable


        //Foreign keys
        [Range(0, int.MaxValue, ErrorMessage = "InventoryRecordId must be 0 or positive.")]
        public int InventoryRecordId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "UserId must be 0 or positive.")]
        public int UserId { get; set; }


        //Navigation properties
        public InventoryRecord? InventoryRecord { get; set; }
        public User? User { get; set; }



        //Custom validation methods

        //Validate DueDate is not before BorrowDate
        public static ValidationResult? ValidateDates(BorrowRecord record, ValidationContext context)
        {
            if (record.DueDate < record.BorrowDate)
                return new ValidationResult("Due date cannot be earlier than borrow date.");
            if (record.BorrowDate > DateOnly.FromDateTime(DateTime.Now))
                return new ValidationResult("Borrow date cannot be in the future.");
            return ValidationResult.Success;
        }

        //Validate ReturnDate is not before BorrowDate
        public static ValidationResult? ValidateReturnDate(DateOnly? returnDate, ValidationContext context)
        {
            if (returnDate == null) return ValidationResult.Success;

            var record = (BorrowRecord)context.ObjectInstance;
            if (returnDate < record.BorrowDate)
                return new ValidationResult("Return date cannot be before borrow date.");

            return ValidationResult.Success;
        }
    }
}
