using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Web.OAuthAuthorizationServer.JsonWebToken
{
    // http://tools.ietf.org/html/draft-jones-json-web-token-10
    public class JWT
    {
        private readonly byte[] _key;

        public JWTHeader Header { get; set; }

        public Dictionary<string, object> Claims { get; set; }

        public DateTime IssuedDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

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

            if (String.IsNullOrWhiteSpace(jwtString))
                throw new ArgumentException("Token cannot be empty", "jwtString");

            var segments = jwtString.Split('.');
            if (segments.Length < 3)
                throw new ArgumentException("Token was invalid", "jwtString");

            var header = segments[0];
            var payload = segments[1];
            var signature = segments[2];

            //checking signature
            var stringToSing = header + "." + payload;
            var signatureCheck = SignString(stringToSing);

            if (signatureCheck != signature)
                throw new InvalidTokenException();

            // getting back to json format
            var headerJson = Encoding.UTF8.GetString(Base64Helper.Base64UrlDecode(header));
            var payloadJson = Encoding.UTF8.GetString(Base64Helper.Base64UrlDecode(payload));

            // converting json to object models
            Header = JsonConvert.DeserializeObject<JWTHeader>(headerJson);

            var claims = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson);

            // extracting issued date
            var issuedDate = DateTime.MinValue;
            if (claims.ContainsKey("nbf") && (claims["nbf"] is long))
            {
                var nbf = (long)claims["nbf"];
                issuedDate = nbf.UnixTimestampToDateTime();
            }

            IssuedDate = issuedDate;

            // extracting expiration date
            var expDate = DateTime.MinValue;
            if (claims.ContainsKey("exp") && (claims["exp"] is long))
            {
                var exp = (long)claims["exp"];
                expDate = exp.UnixTimestampToDateTime();
            }

            ExpirationDate = expDate;
        }

        /// <summary>
        /// Returns a JWT encoded string based on properties
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        private string SignString(string stringToSign)
        {
            using (var sha256 = new HMACSHA256(_key))
            {
                var buffer = Encoding.UTF8.GetBytes(stringToSign);
                var cipher = sha256.ComputeHash(buffer);

                return Base64Helper.Base64UrlEncode(cipher);
            }
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