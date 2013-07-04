using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zazz.Web.OAuthAuthorizationServer.JsonWebToken
{
    // http://tools.ietf.org/html/draft-jones-json-web-token-10
    public class JWT
    {
        private byte[] _key;

        public JWTHeader Header { get; set; }

        public Dictionary<string, object> Claims { get; set; }

        public DateTime IssuedTime { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public string Signature { get; set; }

        public JWT()
        {
            // 64 bytes
            const string KEY = "3iTZUAxSiDc4QoOS8UWzy1JTgQ6Z2H0hvIZMWtkaTqkCbnNProQH3jv/4HlsG0CcvmbAaubWTLgoGxuwfeQEZQ==";
            _key = Convert.FromBase64String(KEY);
        }

        /// <summary>
        /// Decodes an existing JWT string and populates the properties
        /// </summary>
        /// <param name="jwtString">Json Web Token encoded string.</param>
        public JWT(string jwtString) : this()
        {
            if (String.IsNullOrWhiteSpace(jwtString))
                throw new ArgumentNullException("jwtString");
        }

        /// <summary>
        /// Returns a JWT encoded string based on properties
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public class JWTHeader
    {
        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("typ")]
        public string Type { get { return "JWT"; } }

        /// <summary>
        /// Algorithm
        /// </summary>
        [JsonProperty("alg")]
        public string Algorithm { get; set; }
    }
}