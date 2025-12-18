
namespace Library.Shared.DTOs.BorrowRecord
{
    public class BorrowListDto
    {
        public int Id { get; set; }

        public DateOnly BorrowDate { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public bool IsOverdue { get; set; }
        public int OverdueDays { get; set; }

        public int BookdId { get; set; }
        public string? CopyCode { get; set; }

        public int UserId { get; set; }
        public string? Username { get; set; }
    }
}
