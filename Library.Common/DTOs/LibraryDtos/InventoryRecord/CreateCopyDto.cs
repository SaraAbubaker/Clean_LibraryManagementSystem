using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.LibraryDtos.InventoryRecord
{
    public class CreateCopyDto
    {
        [Required]
        [Positive]
        public int BookId { get; set; }

    }
}
