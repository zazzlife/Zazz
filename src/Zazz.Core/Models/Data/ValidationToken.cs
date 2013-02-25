using System;

namespace Zazz.Core.Models.Data
{
    public class ValidationToken
    {
        public int Token { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}