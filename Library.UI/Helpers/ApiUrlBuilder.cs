namespace Library.UI.Helpers
{
    public static class ApiUrlBuilder
    {
        private static string Normalize(string path)
            => path.TrimEnd('/');

        public static string ForQuery(string basePath)
            => $"{Normalize(basePath)}/query";

        public static string ForId(string basePath, int id, int? userId = null)
        {
            var path = $"{Normalize(basePath)}/{id}";
            return userId.HasValue
                ? $"{path}?userId={userId}"
                : path;
        }
    }
}
