using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure;

namespace Zazz.UnitTests.Infrastructure
{
    [TestFixture]
    public class ErrorHandlerShould
    {
        private Mock<IUoW> _uow;
        private Mock<ILogger> _logger;
        private Mock<IEmailService> _emailService;
        private ErrorHandler _sut;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _logger = new Mock<ILogger>();
            _emailService = new Mock<IEmailService>();
            _sut = new ErrorHandler(_uow.Object, _logger.Object, _emailService.Object);

            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }
    }
}