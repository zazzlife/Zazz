using System;
using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class PartyAlbumViewModel
    {
        public DateTime CreatedDate { get; set; }

        public IEnumerable<PhotoViewModel> Photos { get; set; }
    }
}