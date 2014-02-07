using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks ProfileImage { get; set; }
    }
}