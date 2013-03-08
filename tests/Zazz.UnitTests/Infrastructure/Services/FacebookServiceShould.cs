using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class FacebookServiceShould
    {
        private Mock<IFacebookHelper> _fbHelper;
        private FacebookService _sut;

        [SetUp]
        public void Init()
        {
            _fbHelper = new Mock<IFacebookHelper>();
            _sut = new FacebookService(_fbHelper.Object);

            _fbHelper.Setup(x => x.SetAccessToken(It.IsAny<string>()));
        }

        [Test]
        public async Task CallRightPathAndSetAccessToken_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Returns(() => Task.Run(() => user));

            //Act
            var result = await _sut.GetUserAsync(id, token);

            //Assert
            _fbHelper.Verify(x => x.SetAccessToken(token), Times.Once());
            _fbHelper.Verify(x => x.GetAsync<FbUser>(id, "email"), Times.Once());
            Assert.AreSame(user, result);
        }


    }
}