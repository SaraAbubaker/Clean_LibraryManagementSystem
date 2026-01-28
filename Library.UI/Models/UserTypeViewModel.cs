using System.Collections.Generic;
using System.Net;
using Library.Common.RabbitMqMessages.UserTypeMessages;

namespace Library.UI.Models
{
    public class UserTypeViewModel
    {
        public List<UserTypeListMessage> UserTypes { get; set; } = new List<UserTypeListMessage>();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public string? ApiResponse { get; set; }
    }
}