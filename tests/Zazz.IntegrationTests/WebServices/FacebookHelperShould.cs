using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;

namespace Zazz.IntegrationTests.WebServices
{
    [TestFixture, Explicit("Valid access token is required")]
    public class FacebookHelperShould
    {
        private string _facebookClientId = "";
        private string _facebookClientSecret = "";
        
        [Test, Explicit("This test is only for manual checking")]
        public void GetEvents_OnGetEvents()
        {
            //Arrange
            var accessToken = "AAACEdEose0cBANC156cTAhp4X74G90XRQqZCulMIdwoQDkH6QDnumEwBhhXqezFSMhWbKaagvcQv8J4TZApZAZAfFIfCzFgrmWjM5wt7GzsK3dUm0B8O";
            var userId = 100004326581895;

            var sut = new FacebookHelper(new KeyChain("", _facebookClientId, _facebookClientSecret));
            
            //Act
            var result = sut.GetEvents(userId, accessToken);

            //Assert

        }

        [Test, Explicit("This test is only for manual checking")]
        public void ManualTest()
        {
            //Arrange
            var sut = new FacebookHelper(new KeyChain("", _facebookClientId, _facebookClientSecret));

            //Act

            //Assert
            Assert.Pass("This is only for debugging");
        }


    }
}