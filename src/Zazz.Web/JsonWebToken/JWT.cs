using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zazz.Web.JsonWebToken
{
    public class JWT
    {
        public JWTHeader Header { get; set; }

        public Dictionary<string, object> Claims { get; set; }

        public DateTime IssuedTime { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public string Signature { get; set; }
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