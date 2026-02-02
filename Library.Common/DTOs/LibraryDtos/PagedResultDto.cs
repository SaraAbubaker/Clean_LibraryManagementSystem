using System.Text.Json.Serialization;

namespace Library.Common.DTOs.LibraryDtos
{
    public class PagedResultDto<T>
    {
        [JsonPropertyOrder(1)]
        public int Page { get; set; }

        [JsonPropertyOrder(2)]
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        [JsonPropertyOrder(3)]
        public int TotalCount { get; set; }

        [JsonPropertyOrder(4)]
        public int PageSize { get; set; }

        [JsonPropertyOrder(5)]
        public List<T> Items { get; set; } = new();
    }
}
