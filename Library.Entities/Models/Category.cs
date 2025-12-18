
using Library.Entities.Base;

using System.ComponentModel.DataAnnotations;

namespace Library.Entities.Models
{
    public class Category : AuditBase, IArchivable
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        //many books = 1 category
        public List<Book> Books { get; set; } = new();
    }
}
