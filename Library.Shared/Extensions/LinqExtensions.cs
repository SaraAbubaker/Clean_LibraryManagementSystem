
namespace Library.Shared.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> source,
            bool condition,
            Func<T, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return condition ? source.Where(predicate) : source;
        }
    }
}

