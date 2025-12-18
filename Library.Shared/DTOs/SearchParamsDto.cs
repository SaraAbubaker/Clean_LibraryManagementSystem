
using System.Text.Json.Serialization;

namespace Library.Shared.DTOs
{
    public class SearchParamsDto
    {

        public string? SearchParam { get; set; } //Title, Author, etc

        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 3;

        // Sorting
        public enum BookSortBy
        {
            Id,
            Title,
            PublishDate,
            Author,
            Category,
            Publisher
        }

        public enum SortDirection
        {
            Asc,
            Desc
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookSortBy SortBy { get; set; } = BookSortBy.Title;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SortDirection SortDir { get; set; } = SortDirection.Asc;
    }
}
