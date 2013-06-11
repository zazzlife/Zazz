using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models
{
    public class UserSearchResult
    {
        public int UserId { get; set; }

        public AccountType AccountType { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }
    }
}