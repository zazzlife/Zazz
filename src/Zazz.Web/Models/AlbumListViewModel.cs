using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class AlbumListViewModel
    {
        [StringLength(50)]
        public string AlbumName { get; set; }

        public IEnumerable<AlbumViewModel> Albums { get; set; }
    }
}