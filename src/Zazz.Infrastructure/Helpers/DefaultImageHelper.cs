using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Helpers
{
    public class DefaultImageHelper
    {
        public static PhotoLinks GetUserDefaultImage(Gender gender)
        {
            return new PhotoLinks
                   {
                       VerySmallLink = "/Images/placeholder.gif",
                       SmallLink = "/Images/placeholder.gif",
                       MediumLink = "/Images/placeholder.gif",
                       OriginalLink = "/Images/placeholder.gif",
                   };
        }

        public static string GetDefaultCoverImage()
        {
            return "/Images/cover-image.gif";
        }

        public static PhotoLinks GetDefaultAlbumImage()
        {
            return new PhotoLinks("/Images/placeholder.gif");
        }

        public static PhotoLinks GetDefaultEventImage()
        {
            return new PhotoLinks("/Images/placeholder.gif");
        }
    }
}