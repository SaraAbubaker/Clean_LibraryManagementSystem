
namespace Library.Common.RabbitMqMessages.UserMessages
{
    public class UserListMessage
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;

        public int UserTypeId { get; set; }
        public string UserRole { get; set; } = null!;

        public int BorrowedBooksCount { get; set; }

        public string Token { get; set; } = null!;
        public bool IsArchived { get; set; } = false;
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}