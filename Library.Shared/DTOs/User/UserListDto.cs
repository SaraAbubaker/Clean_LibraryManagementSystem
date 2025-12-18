
namespace Library.Shared.DTOs.User
{
    public class UserListDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;

        public int UserTypeId { get; set; }
        public string UserRole { get; set; } = null!;
        public int BorrowedBooksCount { get; set; }
    }
}
