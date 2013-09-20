using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class AlbumsListViewModel
    {
        public bool IsForCurrentUser { get; set; }

        public List<AlbumViewModel> Albums { get; set; }
    }
}