
using Library.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.BorrowRecord
{
    public class ReturnBorrowDto
    {
        [Required]
        [Positive]
        public int BorrowRecordId { get; set; }
    }
}
