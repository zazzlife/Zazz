using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.OAuthAuthorizationServer
{
    [TestFixture]
    public class OAuthServiceShould
    {
        private MockRepository _mocRepo;
        private Mock<IUoW> _uow;
        private OAuthService _sut;

        [SetUp]
        public void Init()
        {
            _mocRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mocRepo.Create<IUoW>();

            
        }
    }
}