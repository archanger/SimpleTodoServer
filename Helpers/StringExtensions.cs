namespace TodoAPI.Helpers
{
    public static class StringExtensions
    {
        public static string ToFirstLetterUppercased(this string origin) 
        {
            if (string.IsNullOrEmpty(origin)) {
                return origin;
            }

            if (origin.Length < 2) {
                return origin.ToUpper();
            }

            return char.ToUpper(origin[0]) + origin.Substring(1);
        }
    }
}