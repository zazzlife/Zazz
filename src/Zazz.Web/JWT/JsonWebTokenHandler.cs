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

        public JsonWebTokenHandler(byte[] key)
        {
            _key = key;
        }

        public JsonWebTokenHandler(string key)
        {
            _key = Convert.FromBase64String(key);
        }

        public string Encode(HashSet<KeyValuePair<string, object>> claims, DateTime? expirationData = null)
        {
            throw new NotImplementedException();
        }

        public JsonWebToken Decode(string token)
        {
            throw new NotImplementedException();
        }
    }
}