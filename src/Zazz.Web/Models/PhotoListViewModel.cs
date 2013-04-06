using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class PhotoListViewModel
    {
        public int UserId { get; set; }

        public IEnumerable<PhotoViewModel> Photos { get; set; } 
    }
}