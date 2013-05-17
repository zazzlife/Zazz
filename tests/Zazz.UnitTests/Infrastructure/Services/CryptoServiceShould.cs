using System;
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
        public void AlwaysGenerate28Characters_ForPasswordSigning()
        {
            //Arrange & Act & Assert
            var randomBytes = new RNGCryptoServiceProvider();
            var rnd = new Random();

            for (int i = 0; i < 500; i++)
            {
                var randomData = new byte[rnd.Next(20, 500)];
                randomBytes.GetBytes(randomData);

                var text = Encoding.UTF8.GetString(randomData);
                var hash = _sut.GeneratePasswordHash(text);

                Assert.AreEqual(28, hash.Length);
            }
        }

        [Test]
        public void ThrowExceptionWhenPasswordIsNullOrEmpty_OnGeneratePasswordHash([Values(null, "")] string pass)
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => _sut.GeneratePasswordHash(pass));
        }

        [TestCase("Soroush", "foEQUgWMeaIb+sZ7wnq+rzaTKSQ=")]
        [TestCase("Test", "bFckl+N4Cyawjh5qBhuSZwTIhRc=")]
        [TestCase(" ", "obfrqmcoMlblAhU1U6MbbSPg77k=")]
        [TestCase("TEST", "YkS9cEtjYIOThH24GfMl46q4jD8=")]
        public void GenerateExpectedValues_OnGeneratePasswordHash(string text, string expected)
        {
            //Act
            var hash = _sut.GeneratePasswordHash(text);

            //Assert
            Assert.AreEqual(expected, hash);
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
    }
}