using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiPartyWebFollow
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }
    }
}