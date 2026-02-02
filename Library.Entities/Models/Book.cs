
using System.ComponentModel.DataAnnotations;
using Library.Common.Base;

namespace Library.Entities.Models
{
    public class Book : AuditBase, IArchivable
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = null!;

        [Required]
        public DateOnly PublishDate { get; set; }

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
