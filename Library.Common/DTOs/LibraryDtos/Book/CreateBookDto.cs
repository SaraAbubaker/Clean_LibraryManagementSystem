using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.Book
{
    public class CreateBookDto : IValidatableObject
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = null!;
        [Required]
        public DateOnly PublishDate { get; set; }

        [RegularExpression(@"^\d+(\.\d+){0,2}$", ErrorMessage = "Version must be in format 1.0 or 1.0.0")]
        [StringLength(50)]
        public string? Version { get; set; }


        [Positive]
        public int? PublisherId { get; set; }

        [Positive]
        public int? AuthorId { get; set; }

        [Positive]
        public int? CategoryId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PublishDate > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                yield return new ValidationResult("Publish date cannot be in the future.", new[] { nameof(PublishDate) });
            }
        }
    }
}
