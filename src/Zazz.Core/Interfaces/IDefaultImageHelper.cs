using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IDefaultImageHelper
    {
        PhotoLinks GetUserDefaultImage(Gender gender);

        PhotoLinks GetDefaultCoverImage();

        PhotoLinks GetDefaultAlbumImage();

        PhotoLinks GetDefaultEventImage();

        PhotoLinks GetDefaultWeeklyImage();
    }
}