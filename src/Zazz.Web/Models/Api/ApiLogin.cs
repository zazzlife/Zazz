using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiLoginRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public int AppId { get; set; }
    }

    public class ApiLoginResponse
    {
        public int Id { get; set; }

        public AccountType AccountType { get; set; }

        public bool IsConfirmed { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }
    }
}