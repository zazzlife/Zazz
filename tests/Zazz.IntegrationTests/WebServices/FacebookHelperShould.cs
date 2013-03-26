using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;

namespace Zazz.IntegrationTests.WebServices
{
    [TestFixture]
    public class FacebookHelperShould
    {
        [Test]
        public async Task ReturnCorrectValues()
        {
            //Arrange
            var sut = new FacebookHelper();

            //Act
            var result = await sut.GetAsync<FbUser>("Soroush.Mirzaei");

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1100457108, long.Parse(result.Id));
            Assert.AreEqual("Soroush.Mirzaei", result.Username);
            Assert.IsNotNull(result.Name);
        }

        [Test, Explicit("Valid access token is required")]
        public async Task GetCorrectFieldsWhenSpecified()
        {
            //Arrange
            const string ACCESS_TOKEN = "";
            var sut = new FacebookHelper();
            sut.SetAccessToken(ACCESS_TOKEN);
            //Act
            var result = await sut.GetAsync<FbUser>("me", "email", "username");

            //Assert
            Assert.IsNotNull(result.Email);
            Assert.IsNotNull(result.Username);
        }


    }
}