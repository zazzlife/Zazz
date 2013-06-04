using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiFollower
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }
    }
}