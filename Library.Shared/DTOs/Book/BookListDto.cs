
namespace Library.Shared.DTOs.Book
{
    public class BookListDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public DateOnly PublishDate { get; set; }
        public string? Version { get; set; }

        public string PublisherName { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public bool IsAvailable { get; set; }
    }
}
