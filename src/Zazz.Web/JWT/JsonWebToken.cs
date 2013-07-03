using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zazz.Web.JWT
{
    public class JsonWebToken
    {
        public JWTHeader Header { get; set; }

        public HashSet<Dictionary<string, object>> Claims { get; set; }

        public DateTime IssuedTime { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public string Signature { get; set; }
    }

    public class JWTHeader
    {
        /// <summary>
        /// Type
        /// </summary>
        public string typ { get { return "JWT"; } }

        /// <summary>
        /// Algorithm
        /// </summary>
        public string alg { get; set; }
    }
}