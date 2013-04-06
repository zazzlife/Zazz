using System.Collections.Generic;

namespace Zazz.Web.Models
{
    internal class MainPhotoPageViewModel
    {
        public PhotoViewType ViewType { get; set; }

        public bool IsForOwner { get; set; }

        public int UserId { get; set; }

        public IEnumerable<PhotoViewModel> Photos { get; set; }

        public IEnumerable<AlbumViewModel> Albums { get; set; }
    }

    internal enum PhotoViewType : byte
    {
        Photos,
        Albums
    }

    internal class AlbumViewModel
    {
        public bool IsOwner { get; set; }

        public int AlbumId { get; set; }

        public int UserId { get; set; }

        public string AlbumName { get; set; }
    }

    internal class PhotoViewModel
    {
        public int PhotoId { get; set; }

        public string PhotoDescription { get; set; }

        public string PhotoUrl { get; set; }

        public bool IsFromCurrentUser { get; set; }
    }
}