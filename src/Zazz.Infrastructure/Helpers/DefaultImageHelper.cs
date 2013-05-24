using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Helpers
{
    public class DefaultImageHelper
    {
        public static PhotoLinks GetUserDefaultImage(Gender gender, string baseAddress = "")
        {
            return new PhotoLinks(baseAddress + "/Images/placeholder.gif");
        }

        public static string GetDefaultCoverImage(string baseAddress = "")
        {
            return baseAddress + "/Images/cover-image.gif";
        }

        public static PhotoLinks GetDefaultAlbumImage(string baseAddress = "")
        {
            return new PhotoLinks(baseAddress + "/Images/placeholder.gif");
        }

        public static PhotoLinks GetDefaultEventImage(string baseAddress = "")
        {
            return new PhotoLinks(baseAddress + "/Images/placeholder.gif");
        }

        public static PhotoLinks GetDefaultWeeklyImage(string baseAddress = "")
        {
            return new PhotoLinks(baseAddress + "/Images/placeholder.gif");
        }
    }
}