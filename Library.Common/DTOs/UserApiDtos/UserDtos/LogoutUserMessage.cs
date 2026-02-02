namespace Library.Common.DTOs.UserApiDtos.UserDtos
{
    public class LogoutUserMessage
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public DateTime LoggedOutAt { get; set; }
        public string Status { get; set; } = "Success";
    }
}