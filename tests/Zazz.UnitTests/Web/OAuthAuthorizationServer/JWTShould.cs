using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Zazz.Core.Exceptions;
using Zazz.Infrastructure.Helpers;
using Zazz.Web;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.OAuthAuthorizationServer
{
    [TestFixture]
    public class JsonWebTokenHandlerShould
    {
        [Test]
        public void CreateCorrectHeader_OnEncode()
        {
            //Arrange
            var jwt = new JWT
                      {
                          Claims = new ConcurrentDictionary<string, object>(),
                          ExpirationDate = DateTime.UtcNow.AddHours(1)
                      };

            jwt.Claims.TryAdd("stringKey", "stringVal");
            jwt.Claims.TryAdd("intKey", 1234);

            //Act
            var result = jwt.ToJWTString();

            var segments = result.Split('.');
            dynamic header = JObject.Parse(Encoding.UTF8.GetString(Base64Helper.Base64UrlDecode(segments[0])));

            //Assert
            Assert.AreEqual("HS256", (string)header.alg);
            Assert.AreEqual("JWT", (string)header.typ);
        }

        [Test]
        public void CreateCorrectClaims_OnEncode()
        {
            //Arrange
            var jwt = new JWT
                      {
                          Claims = new ConcurrentDictionary<string, object>(),
                          ExpirationDate = DateTime.UtcNow.AddHours(1)
                      };

            jwt.Claims.TryAdd("stringKey", "stringVal");
            jwt.Claims.TryAdd("intKey", 1234);
            //Act
            var result = jwt.ToJWTString();

            var segments = result.Split('.');
            dynamic claimsJson = JObject.Parse(Encoding.UTF8.GetString(Base64Helper.Base64UrlDecode(segments[1])));

            //Assert
            Assert.AreEqual("stringVal", (string)claimsJson.stringKey);
            Assert.AreEqual(1234, (int)claimsJson.intKey);
            Assert.AreEqual(jwt.ExpirationDate.Value.ToUnixTimestamp(), (long)claimsJson.exp);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("asdf")]
        [TestCase("asdfg.asdfg")]
        public void ThrowInvalidTokenIfItDoesntHave3Segments_OnDecode(string token)
        {
            //Arrange
            //Act & Assert
            Assert.Throws<ArgumentException>(() => new JWT(token));

        }

        [Test]
        public void ThrowInvalidTokenExceptionIfSignatureIsInvalid_OnDecode()
        {
            //Arrange
            var jwt = new JWT
                      {
                          Claims = new ConcurrentDictionary<string, object>(),
                          ExpirationDate = DateTime.UtcNow.AddDays(1)
                      };

            var validToken = jwt.ToJWTString();
            var segments = validToken.Split('.');
            segments[2] = "invalid token";

            var tokenWithInvalidSign = String.Join(".", segments);

            //Act & Assert
            Assert.Throws<InvalidTokenException>(() => new JWT(tokenWithInvalidSign));
        }

        [Test]
        public void DecodeCorrectly_OnDecode()
        {
            //Arrange
            var jwt = new JWT
                      {
                          Claims = new ConcurrentDictionary<string, object>(),
                          ExpirationDate = DateTime.UtcNow.AddDays(1),
                          TokenType = JWT.ACCESS_TOKEN_TYPE
                      };

            jwt.Claims.TryAdd("key", "val");
            jwt.Claims.TryAdd("key2", 1);

            var validToken = jwt.ToJWTString();

            //Act
            var result = new JWT(validToken);

            //Assert
            Assert.AreEqual("HS256", result.Header.Algorithm);
            Assert.AreEqual("JWT", result.Header.Type);

            Assert.AreEqual(jwt.ExpirationDate.Value.Date, result.ExpirationDate.Value.Date);

            Assert.AreEqual("val", result.Claims["key"]);
            Assert.AreEqual(1, result.Claims["key2"]);
        }

        [Test]
        public void AddAndReadDefaultProperties()
        {
            //Arrange
            var jwt = new JWT
                      {
                          ClientId = 44,
                          ExpirationDate = DateTime.UtcNow,
                          Scopes = new List<string> {"s1", "s2"},
                          TokenType = JWT.REFRESH_TOKEN_TYPE,
                          UserId = 33,
                          VerificationCode = "verification code",
                          TokenId = 443
                      };

            //Act
            var token = jwt.ToJWTString();
            var decodedToken = new JWT(token);

            //Assert
            Assert.AreEqual(jwt.ClientId, decodedToken.ClientId);
            Assert.AreEqual(jwt.ExpirationDate.Value.Date, decodedToken.ExpirationDate.Value.Date);
            CollectionAssert.AreEqual(jwt.Scopes, decodedToken.Scopes);
            Assert.AreEqual(jwt.TokenType, decodedToken.TokenType);
            Assert.AreEqual(jwt.UserId, decodedToken.UserId);
            Assert.AreEqual(jwt.VerificationCode, decodedToken.VerificationCode);
            Assert.AreEqual(jwt.TokenId, decodedToken.TokenId);
        }
    }
}