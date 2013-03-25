using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure
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

        public static string GetDefaultAlbumImage()
        {
            return "/Images/placeholder.gif";
        }
    }
}