
using Library.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Shared.DTOs.InventoryRecord
{
    public class CreateCopyDto
    {
        [Required]
        [Positive]
        public int BookId { get; set; }

    }
}
