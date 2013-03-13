using PagedList;

namespace Zazz.Web.Models
{
    public class AlbumPhotosViewModel
    {
        public bool IsOwner { get; set; }

        public int AlbumId { get; set; }

        public IPagedList<ImageViewModel> Photos { get; set; }
    }
}