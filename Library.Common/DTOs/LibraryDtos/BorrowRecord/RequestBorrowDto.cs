using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.BorrowRecord
{
    public class RequestBorrowDto
    {
        [Required]
        [Positive]
        public int BookId { get; set; }

        //User picks 
        public DateOnly? DueDate { get; set; }
    }
}
