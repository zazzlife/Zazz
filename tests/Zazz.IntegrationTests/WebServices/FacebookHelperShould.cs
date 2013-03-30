using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;

namespace Zazz.IntegrationTests.WebServices
{
    [TestFixture, Explicit("Valid access token is required")]
    public class FacebookHelperShould
    {
        [Test, Explicit("This test is only for manual checking")]
        public async Task GetEvents_OnGetEvents()
        {
            //Arrange
            var accessToken = "AAACEdEose0cBANC156cTAhp4X74G90XRQqZCulMIdwoQDkH6QDnumEwBhhXqezFSMhWbKaagvcQv8J4TZApZAZAfFIfCzFgrmWjM5wt7GzsK3dUm0B8O";
            var userId = 100004326581895;

            var sut = new FacebookHelper();
            
            //Act
            var result = sut.GetEvents(userId, accessToken);

            //Assert

        }

        [Test, Explicit("This test is only for manual checking")]
        public void ManualTest()
        {
            //Arrange
            var helper = new FacebookHelper();

            //Act


            //Assert
            Assert.Pass("This is only for debugging");
        }


    }
}