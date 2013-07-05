using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Web.OAuthAuthorizationServer
{
    // http://tools.ietf.org/html/draft-jones-json-web-token-10
    public class JWT
    {
        private readonly byte[] _key;

        public JWTHeader Header { get; set; }

        public Dictionary<string, object> Claims { get; set; }

        public string Signature { get; private set; }

        
        private const string ISSUED_DATE_KEY = "nbf";
        public DateTime IssuedDate { get; set; }

        private const string EXPIRATION_DATE_KEY = "exp";
        public DateTime? ExpirationDate { get; set; }

        private const string TOKEN_TYPE_KEY = "tokenType";
        public TokenType TokenType { get; set; }

        private const string VERIFY_CODE_KEY = "verify";
        public string VerificationCode { get; set; }
        
        private const string SCOPES_KEY = "scopes";
        public List<string> Scopes { get; set; }

        private const string CLIENT_ID_KEY = "client";
        public int ClientId { get; set; }

        private const string USER_ID_KEY = "usr";
        public int UserId { get; set; }

        public JWT()
        {
            Scopes = new List<string>();

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
            Signature = segments[2];

            //checking signature
            var stringToSing = header + "." + payload;
            var signatureCheck = SignString(stringToSing);

            if (signatureCheck != Signature)
                throw new InvalidTokenException();

            // getting back to json format
            var headerJson = Encoding.UTF8.GetString(Base64Helper.Base64UrlDecode(header));
            var payloadJson = Encoding.UTF8.GetString(Base64Helper.Base64UrlDecode(payload));

            // converting json to object models
            Header = JsonConvert.DeserializeObject<JWTHeader>(headerJson);

            Claims = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson);

            // extracting issued date
            if (Claims.ContainsKey(ISSUED_DATE_KEY) && (Claims[ISSUED_DATE_KEY] is long))
            {
                var nbf = (long)Claims[ISSUED_DATE_KEY];
                IssuedDate = nbf.UnixTimestampToDateTime();
            }

            // extracting expiration date
            if (Claims.ContainsKey(EXPIRATION_DATE_KEY) && (Claims[EXPIRATION_DATE_KEY] is long))
            {
                var exp = (long)Claims[EXPIRATION_DATE_KEY];
                ExpirationDate = exp.UnixTimestampToDateTime();
            }

            // extracting token type
            if (Claims.ContainsKey(TOKEN_TYPE_KEY) && (Claims[TOKEN_TYPE_KEY] is string))
            {
                TokenType t;
                if (Enum.TryParse((string) Claims[TOKEN_TYPE_KEY], true, out t))
                    TokenType = t;
            }

            // extracting verification code
            if (Claims.ContainsKey(VERIFY_CODE_KEY) && (Claims[VERIFY_CODE_KEY] is string))
            {
                VerificationCode = (string) Claims[VERIFY_CODE_KEY];
            }

            // extracting scopes
            if (Claims.ContainsKey(SCOPES_KEY) && (Claims[SCOPES_KEY] is string))
            {
                var scopes = (string) Claims[SCOPES_KEY];
                Scopes = scopes.Split(',').ToList();
            }

            // extracting client id
            if (Claims.ContainsKey(CLIENT_ID_KEY) && (Claims[CLIENT_ID_KEY] is int))
            {
                ClientId = (int) Claims[CLIENT_ID_KEY];
            }

            // extracting user id
            if (Claims.ContainsKey(USER_ID_KEY) && (Claims[USER_ID_KEY] is int))
            {
                UserId = (int) Claims[USER_ID_KEY];
            }
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
            Claims.Add(ISSUED_DATE_KEY, DateTime.UtcNow.ToUnixTimestamp());

            // expiration date
            if (ExpirationDate.HasValue)
                Claims.Add(EXPIRATION_DATE_KEY, ExpirationDate.Value.ToUnixTimestamp());

            // token type
            Claims.Add(TOKEN_TYPE_KEY, TokenType);

            // verify token
            if (!String.IsNullOrWhiteSpace(VerificationCode))
                Claims.Add(VERIFY_CODE_KEY, VerificationCode);

            // scopes
            if (Scopes != null && Scopes.Count > 0)
                Claims.Add(SCOPES_KEY, String.Join(",", Scopes));

            // client id
            Claims.Add(CLIENT_ID_KEY, ClientId);

            // user id
            Claims.Add(USER_ID_KEY, UserId);

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
            Signature = SignString(jwtString);

            jwtString += "." + Signature;

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