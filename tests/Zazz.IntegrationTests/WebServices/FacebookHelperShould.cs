using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;

namespace Zazz.IntegrationTests.WebServices
{
    [TestFixture, Explicit("Valid access token is required")]
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

        [Test]
        public async Task GetEvents_OnGetEvents()
        {
            //Arrange
            var accessToken = "AAACEdEose0cBABaoejBLGpoNNAXsZBoKpoT8dLrLksz2zZCcU25yKg5jTClQ3xwcw9JZCURKxkDZB2Ja21GBD4vSGykZAxOYFpjUESg9ZAoFyGJaF9epmD";
            var userId = 100004326581895;
            var excludeList = new List<int>();

            var sut = new FacebookHelper();
            sut.SetAccessToken(accessToken);
            
            //Act
            var result = await sut.GetEvents(userId, excludeList);

            //Assert

        }


    }
}