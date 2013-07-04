using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zazz.Infrastructure.Helpers
{
    public static class Base64Helper
    {
        // http://tools.ietf.org/html/rfc4648#page-7
        public static string Base64UrlEncode(byte[] data)
        {
            var base64 = Convert.ToBase64String(data);
            base64 = base64.Replace('+', '-');
            base64 = base64.Replace('/', '_');
            base64 = base64.TrimEnd('=');

            return base64;
        }

        // http://tools.ietf.org/html/rfc4648#page-7
        public static byte[] Base64UrlDecode(string base64)
        {
            base64 = base64.Replace('-', '+');
            base64 = base64.Replace('_', '/');

            switch (base64.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: base64 += "=="; break; // Two pad chars
                case 3: base64 += "="; break; // One pad char
                default: throw new ArgumentException("Invalid base64.", "base64");
            }

            return Convert.FromBase64String(base64);
        }
    }
}
