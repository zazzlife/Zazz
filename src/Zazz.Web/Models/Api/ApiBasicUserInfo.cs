using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiBasicUserInfo
    {
        public int UserId { get; set; }

        public AccountType AccountType { get; set; }

        public bool IsConfirmed { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }    
    }
}