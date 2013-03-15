using Zazz.Core.Models.Data;

namespace Zazz.Web.Helpers
{
    public class DefaultImageHelper
    {
        public static string GetUserDefaultImage(Gender gender)
        {
            return "/Images/placeholder.gif";
        }

        public static string GetDefaultCoverImage()
        {
            return "/Images/cover-image.gif";
        }
    }
}