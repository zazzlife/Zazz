using System;

namespace Zazz.Web.Controllers.Api
{
    public class ApiAppToken
    {
        public long RequestId { get; set; }

        public string Token { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}