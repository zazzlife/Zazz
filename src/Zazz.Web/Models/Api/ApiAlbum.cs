using System;
using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiAlbum
    {
        public int AlbumId { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public PhotoLinks Thumbnail { get; set; }

        public DateTime CreatedDate { get; set; }

        public IEnumerable<ApiPhoto> Photos { get; set; }
    }
}