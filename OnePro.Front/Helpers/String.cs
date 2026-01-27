namespace OnePro.Front.Helpers
{
    public static class StringExtensions
    {
        public static string TruncateWords(this string? value, int maxWords)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= maxWords)
                return value;

            var take = string.Join(" ", words.Take(maxWords));
            return take + "...";
        }
    }
}
