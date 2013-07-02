using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zazz.Web.JWT
{
    public class JsonWebToken
    {
        public JWTHeader Header { get; set; }

        public ICollection<KeyValuePair<string, object>> Claims { get; set; }

        public string Signature { get; set; }
    }

    public class JWTHeader
    {
        /// <summary>
        /// Issuer
        /// </summary>
        public string iss { get; set; }

        /// <summary>
        /// Audience
        /// </summary>
        public string aud { get; set; }

        /// <summary>
        /// Not Before
        /// </summary>
        public long nbf { get; set; }

        /// <summary>
        /// Expiration Time
        /// </summary>
        public long exp { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        private string typ { get { return "JWT"; } }

        /// <summary>
        /// Algorithm
        /// </summary>
        public string alg { get; set; }
    }
}