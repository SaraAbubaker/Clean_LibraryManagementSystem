namespace Library.UI.Models.String_constant
{
    public class ApiSettings
    {
        public UserApiSettings UserApi { get; set; } = new();
        public LibraryApiSettings LibraryApi { get; set; } = new();
    }
}