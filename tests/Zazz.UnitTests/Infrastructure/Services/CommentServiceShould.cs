using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class CommentServiceShould
    {
        private Mock<IUoW> _uow;
        private Mock<INotificationService> _notificationService;
        private CommentService _sut;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _notificationService = new Mock<INotificationService>();
            _sut = new CommentService(_uow.Object, _notificationService.Object);
        }
    }
}