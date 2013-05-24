using System;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure
{
    public class DefaultImageHelper : IDefaultImageHelper
    {
        private readonly string _baseAddress;

        public DefaultImageHelper(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public PhotoLinks GetUserDefaultImage(Gender gender)
        {
            return new PhotoLinks(_baseAddress + "/Images/placeholder.gif");
        }

        public PhotoLinks GetDefaultCoverImage()
        {
            return new PhotoLinks(_baseAddress + "/Images/cover-image.gif");
        }

        public PhotoLinks GetDefaultAlbumImage()
        {
            return new PhotoLinks(_baseAddress + "/Images/placeholder.gif");
        }

        public PhotoLinks GetDefaultEventImage()
        {
            return new PhotoLinks(_baseAddress + "/Images/placeholder.gif");
        }

        public PhotoLinks GetDefaultWeeklyImage()
        {
            return new PhotoLinks(_baseAddress + "/Images/placeholder.gif");
        }
    }
}