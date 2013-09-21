using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class AlbumsListViewModel
    {
        /// <summary>
        /// Id of the album owner
        /// </summary>
        public int UserId { get; set; }

        public int CurrentUserId { get; set; }

        public bool IsForCurrentUser { get; set; }

        public List<AlbumViewModel> Albums { get; set; }
    }
}