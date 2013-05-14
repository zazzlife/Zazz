using System;
using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class PartyAlbumViewModel
    {
        public int AlbumId { get; set; }

        public string AlbumName { get; set; }

        public DateTime CreatedDate { get; set; }

        public IEnumerable<PhotoViewModel> Photos { get; set; }
    }
}