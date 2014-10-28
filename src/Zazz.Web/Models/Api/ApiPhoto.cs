using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiPhoto
    {
        public int PhotoId { get; set; }

        public int? AlbumId { get; set; }

        public string Description { get; set; }

        public PhotoLinks PhotoLinks { get; set; }

        public int UserId { get; set; }

        public string UserDisplayName { get; set; }

        public PhotoLinks UserDisplayPhoto { get; set; }

        public IEnumerable<int> Categories { get; set; }

        public string TagUser { get; set; }
    }
}