
namespace Library.Shared.DTOs.ApiResponses
{
    public class ApiPagedResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public T? Data { get; set; }
    }
}
