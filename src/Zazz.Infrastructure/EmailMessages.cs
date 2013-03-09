namespace Zazz.Infrastructure
{
    public static class EmailMessages
    {
        public const string NOREPLY_ADDRESS = "noreply@zazzlife.com";

        public static string AccessTokenExpiredMessage(out bool isHtml, out  string subject)
        {
            subject = "Zazz - Your access token has been expired.";
            isHtml = false;
            return "";
        }
    }
}