using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class ClubRewardServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private ClubRewardService _sut;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();

            _sut = new ClubRewardService(_uow.Object);
        }
    }
}