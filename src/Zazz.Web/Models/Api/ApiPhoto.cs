using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiPhoto
    {
        public int PhotoId { get; set; }

        public string Description { get; set; }

        public PhotoLinks PhotoLinks { get; set; }

        public int UserId { get; set; }

        public string UserDisplayName { get; set; }

        public PhotoLinks UserDisplayPhoto { get; set; }
    }
}