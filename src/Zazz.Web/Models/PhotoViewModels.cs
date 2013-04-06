using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class MainPhotoPageViewModel
    {
        public PhotoViewType ViewType { get; set; }

        public bool IsForOwner { get; set; }

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
        public bool IsOwner { get; set; }

        public int AlbumId { get; set; }

        public int UserId { get; set; }

        public string AlbumName { get; set; }
    }

    public class PhotoViewModel
    {
        public int PhotoId { get; set; }

        public string PhotoDescription { get; set; }

        public string PhotoUrl { get; set; }

        public bool IsFromCurrentUser { get; set; }
    }
}