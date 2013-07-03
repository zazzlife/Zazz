﻿using System;
using NUnit.Framework;
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
        public void ReplacePlusWithDash_OnBase64UrlEncode()
        {
            //Arrange
            Assert.IsTrue(_base64.IndexOf('+') > -1);

            //Act
            var result = _sut.Base64UrlEncode(Convert.FromBase64String(_base64));

            //Assert
            Assert.IsFalse(result.IndexOf('+') > -1);
            Assert.IsTrue(result.IndexOf('-') > -1);
        }

 
    }
}