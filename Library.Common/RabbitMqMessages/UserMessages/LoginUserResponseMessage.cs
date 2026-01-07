namespace Library.Common.RabbitMqMessages.UserMessages
{
    public class LoginUserResponseMessage
    {
        public UserListMessage User { get; set; } = null!;
        public string Token { get; set; } = null!;
        public int BorrowedBooksCount { get; set; }
        public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;
    }
}