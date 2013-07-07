using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zazz.Infrastructure.Helpers;

namespace Zazz.UnitTests.Infrastructure.Helpers
{
    [TestFixture]
    public class Base64HelperShould
    {
        private string _base64;

        [SetUp]
        public void Init()
        {
            _base64 = "7UeoUtefyJLpwrOgxFjR1Qyf+oo0/cDGAftSoBQssLxLmBLCE2dxpkv62Z7+RZ7+URmb2UMO6enwJofXLa+CKw==";
        }

        [Test]
        public void ReplacePlusWithMinus_OnBase64UrlEncode()
        {
            //Arrange
            Assert.IsTrue(_base64.IndexOf('+') > -1);

            //Act
            var result = Base64Helper.Base64UrlEncode(Convert.FromBase64String(_base64));

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
            var result = Base64Helper.Base64UrlEncode(Convert.FromBase64String(_base64));

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
            var result = Base64Helper.Base64UrlEncode(Convert.FromBase64String(_base64));

            //Assert
            Assert.IsFalse(result.EndsWith("="));
        }

        [Test]
        public void DecodeUrlSafeBase64Correctly_OnBase64Decode()
        {
            //Arrange
            var urlSafeBas64 = "7UeoUtefyJLpwrOgxFjR1Qyf-oo0_cDGAftSoBQssLxLmBLCE2dxpkv62Z7-RZ7-URmb2UMO6enwJofXLa-CKw";

            //Act
            var result = Base64Helper.Base64UrlDecode(urlSafeBas64);

            //Assert
            Assert.AreEqual(_base64, Convert.ToBase64String(result));
        }
    }
}
