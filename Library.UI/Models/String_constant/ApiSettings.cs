namespace Library.UI.Models.String_constant
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public Endpoints Endpoints { get; set; } = new();
    }
}