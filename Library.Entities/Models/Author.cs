
using Library.Common.Base;
using System.ComponentModel.DataAnnotations;

namespace Library.Entities.Models
{
    public class Author : AuditBase, IArchivable
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Email { get; set; }

        public List<Book> Books { get; set; } = new();
    }
}