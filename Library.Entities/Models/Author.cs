
using Library.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Library.Entities.Models
{
    public class Author : AuditBase, IArchivable
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        //Email validation
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(255)]
        public string? Email { get; set; }

        public List<Book> Books { get; set; } = new();

    }
}
