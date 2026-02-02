
using System.ComponentModel.DataAnnotations;
using Library.Common.Base;

namespace Library.Entities.Models
{
    public class BorrowRecord : AuditBase
    {
        [Required(ErrorMessage = "Borrow date is required.")]
        public DateOnly BorrowDate { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateOnly DueDate { get; set; }

        public DateOnly? ReturnDate { get; set; } //nullable

        //Foreign keys
        [Range(0, int.MaxValue, ErrorMessage = "InventoryRecordId must be 0 or positive.")]
        public int InventoryRecordId { get; set; }
        public int UserId { get; set; }

        //Navigation properties
        public InventoryRecord? InventoryRecord { get; set; }

    }
}