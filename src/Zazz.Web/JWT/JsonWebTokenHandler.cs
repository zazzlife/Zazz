using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Zazz.Web.JWT
{
    public class JsonWebTokenHandler
    {
        private readonly byte[] _key;

        public JsonWebTokenHandler()
        {
            // 64 bytes
            const string KEY = "3iTZUAxSiDc4QoOS8UWzy1JTgQ6Z2H0hvIZMWtkaTqkCbnNProQH3jv/4HlsG0CcvmbAaubWTLgoGxuwfeQEZQ==";
            _key = Convert.FromBase64String(KEY);
        }

        public string Encode(HashSet<KeyValuePair<string, object>> claims, DateTime? expirationData = null)
        {
            throw new NotImplementedException();
        }

        public JsonWebToken Decode(string token)
        {
            throw new NotImplementedException();
        }

        // http://tools.ietf.org/html/rfc4648
        public string Base64UrlEncode(byte[] data)
        {
            var base64 = Convert.ToBase64String(data);
            base64 = base64.Replace('+', '-');

            return base64;
        }

        // http://tools.ietf.org/html/rfc4648
        public byte[] Base64UrlDecode(string base64)
        {
            throw new NotImplementedException();
        }
    }
}