
namespace Library.Shared.Helpers
{
    public static class CopyCodeGeneratorHelper
    {
        public static string GenerateBookPrefix(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "BK";

            var words = title.ToUpperInvariant()
                             .Split(new[] { ' ', '\t', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(w => w.Any(char.IsLetter))
                             .ToArray();

            string prefix;

            if (words.Length == 0)
            {
                //Take first 2-4 letters from title
                prefix = new string(title.Where(char.IsLetter).Take(4).ToArray());
            }
            else if (words.Length == 1)
            {
                //Single word: take first 4 letters
                prefix = new string(words[0].Where(char.IsLetter).Take(4).ToArray());
            }
            else
            {
                //Multiple words: take first letter of first 4 words
                prefix = string.Concat(words.Take(4).Select(w => w.First())).Trim();
            }

            if (string.IsNullOrEmpty(prefix)) prefix = "BK";

            prefix = new string(prefix.Where(char.IsLetter).ToArray()).ToUpperInvariant();

            // max 4 letters
            if (prefix.Length > 4) prefix = prefix.Substring(0, 4);

            return prefix;
        }

    }
}
