using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.BorrowRecord
{
    public class ReturnBorrowDto
    {
        [Required]
        [Positive]
        public int BorrowRecordId { get; set; }
    }
}
