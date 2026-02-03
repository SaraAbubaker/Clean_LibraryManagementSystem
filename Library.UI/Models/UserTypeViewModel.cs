
using Library.Common.DTOs.UserApiDtos.UserTypeDtos;

namespace Library.UI.Models
{
    public class UserTypeViewModel
    {
        public List<UserTypeListMessage> UserTypes { get; set; } = new List<UserTypeListMessage>();
        public string? ErrorMessage { get; set; }
    }
}