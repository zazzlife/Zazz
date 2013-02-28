using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class AuthServiceShould
    {
        private Mock<IUoW> _uowMock;
        private AuthService _sut;

        [SetUp]
        public void Init()
        {
            _uowMock = new Mock<IUoW>();
            _sut = new AuthService(_uowMock.Object);
        }
    }
}