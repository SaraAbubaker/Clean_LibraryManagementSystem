
namespace Library.Shared.DTOs.InventoryRecord
{
    public class CopyListDto
    {
        public int Id { get; set; }

        public string CopyCode { get; set; } = null!;
        public bool IsAvailable { get; set; }

        public int BookId { get; set; }
        public string? BookTitle { get; set; }
    }
}
