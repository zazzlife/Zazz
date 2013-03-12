using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class AlbumListViewModel
    {
        [Required, StringLength(50), Display(Name = "Album Name")]
        public string AlbumName { get; set; }

        public IEnumerable<AlbumViewModel> Albums { get; set; }
    }
}