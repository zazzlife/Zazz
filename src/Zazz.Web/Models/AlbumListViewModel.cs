using System.ComponentModel.DataAnnotations;
using PagedList;

namespace Zazz.Web.Models
{
    public class AlbumListViewModel
    {
        [Required, StringLength(50), Display(Name = "Album Name")]
        public string AlbumName { get; set; }

        public bool IsOwner { get; set; }

        public int UserId { get; set; }

        public IPagedList<PhotoViewModel> Albums { get; set; }
    }
}