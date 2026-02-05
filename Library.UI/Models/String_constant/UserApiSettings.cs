namespace Library.UI.Models.String_constant
{
    public class UserApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public UserApiEndpoints Endpoints { get; set; } = new();
    }
}
