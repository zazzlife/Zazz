namespace Zazz.Web.Helpers
{
    public static class StringHelper
    {
        public static string ToUrlFriendlyString(this string text)
        {
            return text.Replace(" ", "-").ToLower();
        }
    }
}