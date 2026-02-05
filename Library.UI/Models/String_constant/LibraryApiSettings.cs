namespace Library.UI.Models.String_constant
{
    public class LibraryApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public LibraryApiEndpoints Endpoints { get; set; } = new();
    }
}
