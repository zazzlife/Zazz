using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.JWT;

namespace Zazz.UnitTests.Web.JWT
{
    [TestFixture]
    public class JsonWebTokenHandlerShould
    {
        private JsonWebTokenHandler _sut;
        private string _base64;

        [SetUp]
        public void Init()
        {
            _sut = new JsonWebTokenHandler();
            _base64 = "7UeoUtefyJLpwrOgxFjR1Qyf+oo0/cDGAftSoBQssLxLmBLCE2dxpkv62Z7+RZ7+URmb2UMO6enwJofXLa+CKw==";
        }

        [Test]
        public void ReplacePlusWithMinus_OnBase64UrlEncode()
        {
            //Arrange
            Assert.IsTrue(_base64.IndexOf('+') > -1);

            //Act
            var result = _sut.Base64UrlEncode(Convert.FromBase64String(_base64));

            //Assert
            Assert.IsFalse(result.IndexOf('+') > -1);
            Assert.IsTrue(result.IndexOf('-') > -1);
        }

        [Test]
        public void ReplaceForwardSlashWithUnderline_OnBase64UrlEncode()
        {
            //Arrange
            Assert.IsTrue(_base64.IndexOf('/') > -1);

            //Act
            var result = _sut.Base64UrlEncode(Convert.FromBase64String(_base64));

            //Assert
            Assert.IsFalse(result.IndexOf('/') > -1);
            Assert.IsTrue(result.IndexOf('_') > -1);
        }

        [Test]
        public void RemoveTrailingEqualSymbols_OnBase64UrlEncode()
        {
            //Arrange
            Assert.IsTrue(_base64.EndsWith("="));

            //Act
            var result = _sut.Base64UrlEncode(Convert.FromBase64String(_base64));

            //Assert
            Assert.IsFalse(result.EndsWith("="));
        }

        [Test]
        public void DecodeUrlSafeBase64Correctly_OnBase64Decode()
        {
            //Arrange
            var urlSafeBas64 = "7UeoUtefyJLpwrOgxFjR1Qyf-oo0_cDGAftSoBQssLxLmBLCE2dxpkv62Z7-RZ7-URmb2UMO6enwJofXLa-CKw";

            //Act
            var result = _sut.Base64UrlDecode(urlSafeBas64);

            //Assert
            Assert.AreEqual(_base64, Convert.ToBase64String(result));
        }

        [Test]
        public void CreateCorrectHeader_OnEncode()
        {
            //Arrange
            var claims = new HashSet<KeyValuePair<string, object>>
                         {
                             new KeyValuePair<string, object>("stringKey", "stringVal"),
                             new KeyValuePair<string, object>("intKey", 1234)
                         };

            //Act
            var result = _sut.Encode(claims, DateTime.UtcNow.AddHours(1));

            var segments = result.Split('.');
            dynamic header = JObject.Parse(Encoding.UTF8.GetString(_sut.Base64UrlDecode(segments[0])));

            //Assert
            Assert.AreEqual("HS256", (string) header.alg);
            Assert.AreEqual("JWT", (string) header.typ);
        }

        [Test]
        public void CreateCorrectClaims_OnEncode()
        {
            //Arrange
            var claims = new HashSet<KeyValuePair<string, object>>
                         {
                             new KeyValuePair<string, object>("stringKey", "stringVal"),
                             new KeyValuePair<string, object>("intKey", 1234)
                         };
            var expDate = DateTime.UtcNow.AddHours(1);

            //Act
            var result = _sut.Encode(claims, expDate);

            var segments = result.Split('.');
            dynamic claimsJson = JObject.Parse(Encoding.UTF8.GetString(_sut.Base64UrlDecode(segments[1])));

            //Assert
            Assert.AreEqual("stringVal", (string) claimsJson.stringKey);
            Assert.AreEqual(1234, (int) claimsJson.intKey);
            Assert.AreEqual(expDate.ToUnixTimestamp(), (long) claimsJson.exp);
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
            Assert.Throws<ArgumentException>(() => _sut.Decode(token));

        }


    }
}