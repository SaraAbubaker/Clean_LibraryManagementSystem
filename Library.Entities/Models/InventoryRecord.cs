
using Library.Common.Base;
using System.ComponentModel.DataAnnotations;

namespace Library.Entities.Models
{
    public class InventoryRecord : AuditBase
    {
        public bool IsAvailable { get; set; } = true;

        [Required]
        [RegularExpression(@"^[A-Z]{1,4}-\d{2}$",
        ErrorMessage = "Copy code must follow format XXXX-01 or X-01 (1–4 uppercase letters, dash, two digits).")]
        public string CopyCode { get; set; } = null!;

        //Foreign Key
        public int BookId { get; set; }
        public int PublisherId { get; set; }

        //Navigation
        public Publisher? Publisher { get; set; }
        public Book? Book { get; set; }

        public List<BorrowRecord> BorrowRecords { get; set; } = new();
    }
}
