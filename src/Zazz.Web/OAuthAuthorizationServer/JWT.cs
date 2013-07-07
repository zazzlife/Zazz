using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zazz.Core.Exceptions;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Web.OAuthAuthorizationServer
{
    // http://tools.ietf.org/html/draft-jones-json-web-token-10
    public class JWT
    {
        private readonly byte[] _key;

        public JWTHeader Header { get; set; }

        public ConcurrentDictionary<string, object> Claims { get; set; }

        public string Signature { get; private set; }

        
        private const string ISSUED_DATE_KEY = "nbf";
        public DateTime? IssuedDate { get; set; }

        private const string EXPIRATION_DATE_KEY = "exp";
        public DateTime? ExpirationDate { get; set; }

        private const string TOKEN_ID_KEY = "id";
        public int? TokenId { get; set; }

        public const string REFRESH_TOKEN_TYPE = "refreshToken";
        public const string ACCESS_TOKEN_TYPE = "accessToken";

        private const string TOKEN_TYPE_KEY = "tokenType";
        public string TokenType { get; set; }

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
            Claims = new ConcurrentDictionary<string, object>();

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

            Claims = JsonConvert.DeserializeObject<ConcurrentDictionary<string, object>>(payloadJson);

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

            // extracting token id
            if (Claims.ContainsKey(TOKEN_ID_KEY))
            {
                int i;
                if (Int32.TryParse(Claims[TOKEN_ID_KEY].ToString(), out i))
                    TokenId = i;
            }

            // extracting token type
            if (Claims.ContainsKey(TOKEN_TYPE_KEY))
            {
                TokenType = Claims[TOKEN_TYPE_KEY].ToString();
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
            if (Claims.ContainsKey(CLIENT_ID_KEY))
            {
                int i;
                if (Int32.TryParse(Claims[CLIENT_ID_KEY].ToString(), out i))
                    ClientId = i;
            }

            // extracting user id
            if (Claims.ContainsKey(USER_ID_KEY))
            {
                int i;
                if (Int32.TryParse(Claims[USER_ID_KEY].ToString(), out i))
                    UserId = i;
            }
        }

        /// <summary>
        /// Returns a JWT encoded string based on properties
        /// </summary>
        /// <returns></returns>
        public string ToJWTString()
        {
            var header = new JWTHeader { Algorithm = "HS256" };

            Claims.AddOrUpdate("iss", "https://www.zazzlife.com", (s, o) => "https://www.zazzlife.com");
            Claims.AddOrUpdate("aud", "Zazz clients", (s, o) => "Zazz clients");

            // issued date
            if (IssuedDate.HasValue)
                Claims.AddOrUpdate(ISSUED_DATE_KEY, IssuedDate.Value.ToUnixTimestamp(),
                                   (s, o) => IssuedDate.Value.ToUnixTimestamp());

            // expiration date
            if (ExpirationDate.HasValue)
                Claims.AddOrUpdate(EXPIRATION_DATE_KEY, ExpirationDate.Value.ToUnixTimestamp(),
                    (s, o) => ExpirationDate.Value.ToUnixTimestamp());

            // token id
            if (TokenId.HasValue)
                Claims.AddOrUpdate(TOKEN_ID_KEY, TokenId.Value, (s, o) => TokenId.Value);

            // token type
            Claims.AddOrUpdate(TOKEN_TYPE_KEY, TokenType, (s, o) => TokenType);

            // verify token
            if (!String.IsNullOrWhiteSpace(VerificationCode))
                Claims.AddOrUpdate(VERIFY_CODE_KEY, VerificationCode, (s, o) => VerificationCode);

            // scopes
            if (Scopes != null && Scopes.Count > 0)
                Claims.AddOrUpdate(SCOPES_KEY, String.Join(",", Scopes), (s, o) => String.Join(",", Scopes));

            // client id
            Claims.AddOrUpdate(CLIENT_ID_KEY, ClientId, (s, o) => ClientId);

            // user id
            Claims.AddOrUpdate(USER_ID_KEY, UserId, (s, o) => UserId);

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