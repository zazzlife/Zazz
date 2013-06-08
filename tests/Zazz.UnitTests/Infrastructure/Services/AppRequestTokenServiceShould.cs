using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class AppRequestTokenServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
        }
    }
}