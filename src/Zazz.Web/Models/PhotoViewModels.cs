using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    public class MainPhotoPageViewModel
    {
        public PhotoViewType ViewType { get; set; }

        public bool IsForCurrentUser { get; set; }

        public string UserDisplayName { get; set; }

        /// <summary>
        /// Id of the user that owns the page. (May or may not be the current user)
        /// </summary>
        public int UserId { get; set; }

        public IEnumerable<PhotoViewModel> Photos { get; set; }

        public IEnumerable<AlbumViewModel> Albums { get; set; }
    }

    public enum PhotoViewType : byte
    {
        Photos,
        Albums
    }

    public class AlbumViewModel
    {
        public bool IsFromCurrentUser { get; set; }

        public int AlbumId { get; set; }

        /// <summary>
        /// Id of the user that owns the page. (May or may not be the current user)
        /// </summary>
        public int UserId { get; set; }

        public string AlbumName { get; set; }

        public PhotoLinks AlbumPicUrl { get; set; }
    }

    public class PhotoViewModel
    {
        public int PhotoId { get; set; }

        public int? AlbumId { get; set; }

        public string PhotoDescription { get; set; }

        public PhotoLinks PhotoUrl { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public int FromUserId { get; set; }

        public string FromUserDisplayName { get; set; }

        public PhotoLinks FromUserPhotoUrl { get; set; }
    }
}