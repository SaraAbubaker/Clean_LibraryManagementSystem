namespace Library.UI.Helpers
{
    public static class ApiUrlBuilder
    {
        public static string ForQuery(string basePath)
            => $"{basePath}/query";

        public static string ForId(string basePath, int id, int? userId = null)
            => userId.HasValue
                ? $"{basePath}/{id}?userId={userId}"
                : $"{basePath}/{id}";
    }
}
