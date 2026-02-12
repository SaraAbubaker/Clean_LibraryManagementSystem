
using Library.Common.Base;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.Entities.Models
{
    public class InventoryRecord : AuditBase
    {
        public bool IsAvailable { get; set; } = true;

        [Required]
        [RegularExpression(@"^[A-Z]{1,4}-\d+-\d{2}$",
        ErrorMessage = "Copy code must follow format XXXX-5-01 (1–4 uppercase letters, dash, BookId, dash, two digits).")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Copy code length is invalid.")]
        public string CopyCode { get; set; } = null!;


        //Foreign Key
        public int BookId { get; set; }
        public int PublisherId { get; set; }

        //Navigation
        public Publisher? Publisher { get; set; }
        [JsonIgnore]
        public Book? Book { get; set; }

        public List<BorrowRecord> BorrowRecords { get; set; } = new();
    }
}
