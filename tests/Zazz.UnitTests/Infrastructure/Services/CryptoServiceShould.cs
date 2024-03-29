﻿using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class CryptoServiceShould
    {
        private CryptoService _sut;

        [SetUp]
        public void Init()
        {
            _sut = new CryptoService();
        }

        [Test]
        public void ThrowExceptionWhenStringIsNullOrEmpty_OnGenerateTextSignature([Values(null, "")] string text)
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => _sut.GenerateTextSignature(text));
        }

        [TestCase("Soroush", "7OE6RcIOCmW40uBFm2kxApukqIecGoCpvTJbuvXIurPJfhmpFqf2lfOied9s1qGmlEV1seRshlClOQ5bsaqHyA==")]
        [TestCase("Test", "yK4PaQEVZW3Q7R0euTF58SLyqRzvIwBGiGXQrLGldKr+jiwH+OQEMHEvRFiTvRQVdDeBYaTvDNGOFsqnI+fbyw==")]
        [TestCase(" ", "r/3KyUDRoVbdB+1wFawJbRybF5MFxx0+H0cG4tbISXlJiTf7Ec1zKHx0/Ub+8mqxA9RxV9eFgZkKfu+aZuRE0A==")]
        [TestCase("TEST", "kaQJskDvERcIaiC+9RKr1vkdv/7H/ctRYR/YeCdyVBnX2OR0swPwLOqNuwro0iao3UfsTfH64/BnSl1hZ2SWWQ==")]
        public void GenerateExpectedValues_OnGenerateTextSignature(string text, string expected)
        {
            //Act
            var hash = _sut.GenerateTextSignature(text);

            //Assert
            Assert.AreEqual(expected, hash);
        }

        [TestCase("Soroush", "ECE13A45C20E0A65B8D2E0459B6931029BA4A8879C1A80A9BD325BBAF5C8BAB3C97E19A916A7F695F3A279DF6CD6A1A6944575B1E46C8650A5390E5BB1AA87C8")]
        [TestCase("Test", "C8AE0F690115656DD0ED1D1EB93179F122F2A91CEF2300468865D0ACB1A574AAFE8E2C07F8E40430712F445893BD141574378161A4EF0CD18E16CAA723E7DBCB")]
        [TestCase(" ", "AFFDCAC940D1A156DD07ED7015AC096D1C9B179305C71D3E1F4706E2D6C84979498937FB11CD73287C74FD46FEF26AB103D47157D78581990A7EEF9A66E444D0")]
        [TestCase("TEST", "91A409B240EF1117086A20BEF512ABD6F91DBFFEC7FDCB51611FD87827725419D7D8E474B303F02CEA8DBB0AE8D226A8DD47EC4DF1FAE3F0674A5D6167649659")]
        public void GenerateExpectedValues_OnGenerateHexTextSignature(string text, string expected)
        {
            //Act
            var hash = _sut.GenerateHexTextSignature(text);

            //Assert
            Assert.AreEqual(expected, hash);
        }

        [TestCase("Soroush", "D793844EE149C54A666038F6CEE9D208C1CC7F67")]
        [TestCase("Test", "1963801F189AABE6D09B4C185956C0BA39B8DF81")]
        [TestCase(" ", "79966BEDFB6BA4320DBBFFB259B8909495317C80")]
        [TestCase("TEST", "77EDB25DB5E27153EE0FA5217D1E69E294E3CF7E")]
        public void GenerateExpectedValues_OnGenerateHMACSHA1Hash(string text, string expected)
        {
            //Act
            var hash = _sut.GenerateHMACSHA1Hash(text, "56e97d3e73d03ac9be75d79d0d5e820d");

            //Assert
            Assert.AreEqual(expected, hash);
        }

        [TestCase("Soroush", "jmbNy+UYBp/tFhAKPadxt0+FK2w=")]
        [TestCase("Test", "mP2O6I6QaGNZyM5mSygWCBNl/5U=")]
        [TestCase(" ", "griU/PsvK4oWDSQjGsdhCIBsXE8=")]
        [TestCase("TEST", "Knv/w/VGrrgIT3vZijJwHxhTZ5A=")]
        public void GenerateExpectedValues_OnGenerateHMACSHA1HashWith(string text, string expected)
        {
            //Act
            var key = Convert.FromBase64String("9IyYemRnr5oRDk+l5sPM1yL+tHZ9+sdx15PXSJlOjuD9KMb8Wsev2vB9dCWylHvvBJD++2YY7clgeuISPCA9Ow==");

            var buffer = Encoding.UTF8.GetBytes(text);
            var hash = _sut.GenerateHMACSHA1Hash(buffer, key);

            //Assert
            Assert.AreEqual(expected, hash);
        }

        [Test]
        public void TestingEncryption()
        {
            //Arrange
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var key = new byte[32];
            rngCryptoServiceProvider.GetNonZeroBytes(key);

            var text = "Soroush";
            var iv = String.Empty;

            //Act
            var cipherText = _sut.EncryptText(text, key, out iv);
            var clearText = _sut.DecryptText(cipherText, iv, key);

            //Assert
            Assert.AreNotEqual(text, cipherText);
            Assert.AreEqual(text, clearText);
        }

        [TestCase(512, 64)]
        [TestCase(256, 32)]
        [TestCase(128, 16)]
        [TestCase(64, 8)]
        public void ReturnCorrectKeySize_OnGenerateKey(int keySizeInBites, int keySizeInBytes)
        {
            //Arrange
            //Act
            var result = _sut.GenerateKey(keySizeInBites);

            //Assert
            Assert.AreEqual(keySizeInBytes, result.Length);
        }

        [Test]
        public void GenerateKeyWithNoZeroesWhenSpecified_OnGenerateKey()
        {
            //Arrange
            //Act
            var result = _sut.GenerateKey(512, true);

            //Assert
            CollectionAssert.DoesNotContain(result, 0);
        }

        [TestCase("Soroush", "sm44MvGoV+uJ1H/tH231y8cVLfg=")]
        [TestCase("soroush", "H9i/SSGhUPgg8gAVCC7kcvwUI3o=")]
        [TestCase("Password32", "J5Szmc90RP8640sU05dHjLQMUsw=")]
        [TestCase("P@$$wo0rd", "V6Vte/9cZMNRlz2KQqonUfYhcUc=")]
        [TestCase("ALongPassword&*(^#! ALongPasswo41241rd", "HB94jpDc0Pg+nw56DCG8S5ltWII=")]
        public void GenerateExpectedHash(string pass, string expected)
        {
            //Arrange
            //Act & Assert

            var hash = _sut.GeneratePasswordHash(pass);
            Assert.AreEqual(expected, hash);
        }

        [Test]
        public void GenerateCorrectHash_OnGenerateHMACSHA512()
        {
            //Arrange
            var key = _sut.GenerateKey(128*8);
            var expected = String.Empty;
            var text = Encoding.UTF8.GetBytes("Soroush");

            using (var h = new HMACSHA512(key))
                expected = Convert.ToBase64String(h.ComputeHash(text));


            //Act
            var result = _sut.GenerateHMACSHA512Hash(text, key);

            //Assert
            Assert.AreEqual(expected, result);
        }




        [Test, Explicit("This is not a test, using it only for generating keys")]
        public void ThisIsOnlyForGeneratingKeys()
        {
            const int KEY_SIZE = 64*8;

            var keyBuffer = _sut.GenerateKey(KEY_SIZE, generateNonZero: true);
            //var keyBuffer = _sut.GenerateKey(KEY_SIZE);

            var base64 = Convert.ToBase64String(keyBuffer);
            var urlSafeBase64 = Base64Helper.Base64UrlEncode(keyBuffer);
            Debug.WriteLine(base64);

        }
    }
}