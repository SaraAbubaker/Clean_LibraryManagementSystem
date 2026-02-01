
namespace Library.UI.Services
{
    public interface IApiClient
    {
        Task<T?> GetQueryAsync<T>(string basePath);

        Task<T?> GetByIdAsync<T>(string basePath, int id, int? userId = null);

        Task<HttpResponseMessage> PostAsync<TBody>(string basePath, TBody body, int? userId = null);

        Task<HttpResponseMessage> PutAsync<TBody>(string basePath, int id, TBody body, int? userId = null);

        Task<HttpResponseMessage> PutArchiveAsync(string basePath, int id, int userId);

        Task<T> PostAsync<TBody, T>(string basePath, TBody body, int? userId = null);

    }
}
