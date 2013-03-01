using System;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
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
    }
}