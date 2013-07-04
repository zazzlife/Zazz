using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Infrastructure.JsonWebToken
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

            Claims = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson);

            // extracting issued date
            var issuedDate = DateTime.MinValue;
            if (Claims.ContainsKey("nbf") && (Claims["nbf"] is long))
            {
                var nbf = (long)Claims["nbf"];
                issuedDate = nbf.UnixTimestampToDateTime();
            }

            IssuedDate = issuedDate;

            // extracting expiration date
            var expDate = DateTime.MinValue;
            if (Claims.ContainsKey("exp") && (Claims["exp"] is long))
            {
                var exp = (long)Claims["exp"];
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
            var header = new JWTHeader { Algorithm = "HS256" };

            Claims.Add("iss", "https://www.zazzlife.com");
            Claims.Add("aud", "Zazz clients");
            Claims.Add("nbf", DateTime.UtcNow.ToUnixTimestamp());

            if (ExpirationDate.HasValue)
                Claims.Add("exp", ExpirationDate.Value.ToUnixTimestamp());

            // converting to json
            var headerJson = JsonConvert.SerializeObject(header, Formatting.None);
            var claimsJson = JsonConvert.SerializeObject(Claims, Formatting.None);

            // getting utf8 bytes
            var headerBytes = Encoding.UTF8.GetBytes(headerJson);
            var claimsBytes = Encoding.UTF8.GetBytes(claimsJson);

            // converting to base64 url safe
            var headerBase64 = Base64Helper.Base64UrlEncode(headerBytes);
            var claimsBase64 = Base64Helper.Base64UrlEncode(claimsBytes);

            // signing
            var jwtString = headerBase64 + "." + claimsBase64;
            var signature = SignString(jwtString);

            jwtString += "." + signature;

            return jwtString;
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