using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class FollowServiceShould
    {
        private Mock<IUoW> _uow;
        private FollowService _sut;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new FollowService(_uow.Object);
        }
    }
}