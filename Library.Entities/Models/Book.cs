
using System.ComponentModel.DataAnnotations;
using Library.Entities.Base;

namespace Library.Entities.Models
{
    public class Book : AuditBase, IArchivable
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = null!;


        [Required]
        [CustomValidation(typeof(Book), nameof(ValidatePublishDate))]
        public DateOnly PublishDate { get; set; }

        public static ValidationResult? ValidatePublishDate(DateOnly date, ValidationContext context)
        {
            if (date > DateOnly.FromDateTime(DateTime.Now))
                return new ValidationResult("Publish date cannot be in the future.");
            return ValidationResult.Success;
        }


        [RegularExpression(@"^\d+(\.\d+){0,2}$", ErrorMessage = "Version must be in format 1.0 or 1.0.0")]
        [StringLength(50)]
        public string? Version { get; set; }


        //Foreign Key Relation
        public int AuthorId { get; set; } = -1;
        public int CategoryId { get; set; } = -1;
        public int PublisherId { get; set; } = -1;

        //Easy access to Category.Name
        public Author? Author { get; set; }
        public Category? Category { get; set; }
        public Publisher? Publisher { get; set; }

        //1 book = many records
        public List<InventoryRecord> InventoryRecords { get; set; } = new();
    }
}
