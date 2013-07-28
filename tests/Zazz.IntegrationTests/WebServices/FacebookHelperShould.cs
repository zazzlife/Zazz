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