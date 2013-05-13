using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Helpers
{
    public class DefaultImageHelper
    {
        public static PhotoLinks GetUserDefaultImage(Gender gender)
        {
            return new PhotoLinks("/Images/placeholder.gif");
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

        public static PhotoLinks GetDefaultWeeklyImage()
        {
            return new PhotoLinks("/Images/placeholder.gif");
        }
    }
}