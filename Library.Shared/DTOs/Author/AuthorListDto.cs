
namespace Library.Shared.DTOs.Author
{
    public class AuthorListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }

        public int BookCount { get; set; }
    }
}
