using System;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class AppRequestToken : BaseEntityLong
    {
        public int AppId { get; set; }

        public byte[] Token { get; set; }

        public AppTokenType TokenType { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}