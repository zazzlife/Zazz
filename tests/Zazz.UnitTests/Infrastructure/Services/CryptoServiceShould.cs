using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Zazz.Infrastructure;
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

        [TestCase("Soroush", "x4v5bzGTqggKFVTAlBozEtUJti4=")]
        [TestCase("Test", "zd2pYSnfXScOSWlsdysp5sX5Syw=")]
        [TestCase(" ", "nfzzCf8ZkNUO0sXYCSzYvPfQxLU=")]
        [TestCase("TEST", "gytI6IQGRv1YkY7epefn3Ju6BRs=")]
        public void GenerateExpectedValues_OnGenerateTextSignature(string text, string expected)
        {
            //Act
            var hash = _sut.GenerateTextSignature(text);

            //Assert
            Assert.AreEqual(expected, hash);
        }

        [TestCase("Soroush", "D793844EE149C54A666038F6CEE9D208C1CC7F67")]
        [TestCase("Test", "1963801F189AABE6D09B4C185956C0BA39B8DF81")]
        [TestCase(" ", "79966BEDFB6BA4320DBBFFB259B8909495317C80")]
        [TestCase("TEST", "77EDB25DB5E27153EE0FA5217D1E69E294E3CF7E")]
        public void GenerateExpectedValues_OnComputeSHA1SignedHash(string text, string expected)
        {
            //Act
            var hash = _sut.GenerateSignedSHA1Hash(text, ApiKeys.FACEBOOK_API_SECRET);

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

        [TestCase("Soroush", "J5qMP2WGddHzyRaS11oDDUCWYX0=")]
        [TestCase("Password", "gP09Rdv2hhq+FbekuqzOhKPnjBw=")]
        [TestCase("P@$$wo0rd", "DVupBGS5sxnRXQOA3H/1/hTArqU=")]
        public void GenerateExpectedTokens_OnGenerateQRCodeToken(string password, string expected)
        {
            //Arrange
            var passWordBuffer = Encoding.UTF8.GetBytes(password);

            //Act
            var result = _sut.GenerateQRCodeToken(passWordBuffer);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase("Soroush")]
        [TestCase("Password")]
        [TestCase("P@$$wo0rd")]
        [TestCase("ALongPassword ALongPassword")]
        public void CorrectlyEncryptAndDecryptPasswords(string password)
        {
            //Arrange
            //Act & Assert
            var iv = String.Empty;
            var cipher = _sut.EncryptPassword(password, out iv);

            var ivBytes = Convert.FromBase64String(iv);
            var decryptedPassword = _sut.DecryptPassword(cipher, ivBytes);

            Assert.AreEqual(password, decryptedPassword);

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
            const int KEY_SIZE = 128*8;

            var keyBuffer = _sut.GenerateKey(KEY_SIZE, generateNonZero: true);
            //var keyBuffer = _sut.GenerateKey(KEY_SIZE);

            var base64 = Convert.ToBase64String(keyBuffer);
            Debug.WriteLine(base64);

        }
    }
}