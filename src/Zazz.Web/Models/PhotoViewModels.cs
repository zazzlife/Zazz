using System.Collections.Generic;

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

        public string AlbumPicUrl { get; set; }
    }

    public class PhotoViewModel
    {
        public int PhotoId { get; set; }

        public string PhotoDescription { get; set; }

        public string PhotoUrl { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public int FromUserId { get; set; }

        public string FromUserDisplayName { get; set; }

        public string FromUserPhotoUrl { get; set; }
    }
}