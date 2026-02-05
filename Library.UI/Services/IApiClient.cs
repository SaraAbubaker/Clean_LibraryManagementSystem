
namespace Library.UI.Services
{
    public interface IApiClient
    {
        Task<T?> GetQueryAsync<T>(string basePath, string apiName = "LibraryApi");

        Task<T?> GetAsync<T>(string path, string apiName = "LibraryApi");

        Task<T?> GetByIdAsync<T>(string basePath, int id, int? userId = null, string apiName = "LibraryApi");

        Task<HttpResponseMessage> PostAsync<TBody>(string basePath, TBody body, int? userId = null, string apiName = "LibraryApi");

        Task<HttpResponseMessage> PutAsync<TBody>(string basePath, int id, TBody body, int? userId = null, string apiName = "LibraryApi");

        Task<HttpResponseMessage> PutArchiveAsync(string basePath, int id, int userId, string apiName = "LibraryApi");

        Task<T> PostAsync<TBody, T>(string basePath, TBody body, int? userId = null, string apiName = "LibraryApi");

    }
}
