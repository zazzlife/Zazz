using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Web.JWT
{
    public class JsonWebTokenHandler
    {
        private readonly byte[] _key;

        // http://tools.ietf.org/html/draft-jones-json-web-token-10

        public JsonWebTokenHandler()
        {
            // 64 bytes
            const string KEY = "3iTZUAxSiDc4QoOS8UWzy1JTgQ6Z2H0hvIZMWtkaTqkCbnNProQH3jv/4HlsG0CcvmbAaubWTLgoGxuwfeQEZQ==";
            _key = Convert.FromBase64String(KEY);
        }

        public string Encode(HashSet<KeyValuePair<string, object>> claims, DateTime? expirationDate = null)
        {
            var header = new JWTHeader { alg = "HS256" };

            var payLoad = new Dictionary<string, object>
                          {
                              {"iss", "https://www.zazzlife.com"},
                              {"aud", "Zazz clients"},
                              {"nbf", DateTime.UtcNow.ToUnixTimestamp()}
                          };

            if (expirationDate.HasValue)
                payLoad.Add("exp", expirationDate.Value.ToUnixTimestamp());

            foreach (var claim in claims)
                payLoad.Add(claim.Key, claim.Value);

            // converting to json
            var headerJson = JsonConvert.SerializeObject(header, Formatting.None);
            var payloadJson = JsonConvert.SerializeObject(payLoad, Formatting.None);

            // getting utf8 bytes
            var headerBytes = Encoding.UTF8.GetBytes(headerJson);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

            // converting to base64 url safe
            var headerBase64 = Base64UrlEncode(headerBytes);
            var payloadBase64 = Base64UrlEncode(payloadBytes);

            // signing
            var jwtString = headerBase64 + "." + payloadBase64;
            var signature = SignString(jwtString);

            jwtString += "." + signature;
            return jwtString;
        }

        public JsonWebToken Decode(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty", "token");

            var segments = token.Split('.');
            if (segments.Length < 3)
                throw new ArgumentException("Token was invalid", "token");

            var header = segments[0];
            var payload = segments[1];
            var signature = segments[2];

            //checking signature
            var stringToSing = header + "." + payload;
            var signatureCheck = SignString(stringToSing);

            if (signatureCheck != signature)
               throw new InvalidTokenException();

            // getting back to json format
            var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));

            // converting json to object models
            var h = JsonConvert.DeserializeObject<JWTHeader>(headerJson);
            var claims = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson);

            // extracting issued date
            var issuedDate = DateTime.MinValue;
            if (claims.ContainsKey("nbf") && (claims["nbf"] is long))
            {
                var nbf = (long)claims["nbf"];
                issuedDate = nbf.UnixTimestampToDateTime();
            }

            // extracting expiration date
            var expDate = DateTime.MinValue;
            if (claims.ContainsKey("exp") && (claims["exp"] is long))
            {
                var exp = (long) claims["exp"];
                expDate = exp.UnixTimestampToDateTime();
            }

            return new JsonWebToken
                   {
                       Claims = claims,
                       Header = h,
                       Signature = signature,
                       IssuedTime = issuedDate,
                       ExpirationTime = expDate
                   };
        }

        // http://tools.ietf.org/html/rfc4648#page-7
        public string Base64UrlEncode(byte[] data)
        {
            var base64 = Convert.ToBase64String(data);
            base64 = base64.Replace('+', '-');
            base64 = base64.Replace('/', '_');
            base64 = base64.TrimEnd('=');

            return base64;
        }

        // http://tools.ietf.org/html/rfc4648#page-7
        public byte[] Base64UrlDecode(string base64)
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

        private string SignString(string stringToSign)
        {
            using (var sha256 = new HMACSHA256(_key))
            {
                var buffer = Encoding.UTF8.GetBytes(stringToSign);
                var cipher = sha256.ComputeHash(buffer);

                return Base64UrlEncode(cipher);
            }
        }
    }
}