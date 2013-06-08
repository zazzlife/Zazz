using System;

namespace Zazz.Web.Models.Api
{
    public class ApiAppToken
    {
        public long RequestId { get; set; }

        public string Token { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}