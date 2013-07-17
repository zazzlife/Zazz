using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class TagServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private Mock<IStaticDataRepository> _staticRepo;
        private Mock<ICategoryStatsCache> _cache;
        private CategoryService _sut;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _staticRepo = _mockRepo.Create<IStaticDataRepository>();
            _cache = _mockRepo.Create<ICategoryStatsCache>();

            _sut = new CategoryService(_uow.Object, _staticRepo.Object, _cache.Object);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-2)]
        [TestCase(-3)]
        [TestCase(-4)]
        public void ReturnResultFromCacheIfItWasUpdatedLessThan5MinsAgo_OnGetTagStats(int minutesAgo)
        {
            //Arrange
            var list = new List<CategoryStat>();

            _cache.SetupGet(x => x.LastUpdate)
                  .Returns(DateTime.UtcNow.AddMinutes(minutesAgo));

            _cache.SetupGet(x => x.CategoryStats)
                  .Returns(list);

            //Act
            var result = _sut.GetAllStats();

            //Assert
            Assert.AreSame(list, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void GetNewDataFromRepoAndSaveItToCacheIfCacheIsExpired_OnGetTagStats()
        {
            //Arrange
            var list = new List<CategoryStat>();

            _cache.SetupGet(x => x.LastUpdate)
                  .Returns(DateTime.UtcNow.AddMinutes(-5));

            _uow.Setup(x => x.CategoryStatRepository.GetAll())
                .Returns(() => new EnumerableQuery<CategoryStat>(list));

            _cache.SetupSet(x => x.CategoryStats = list);
            _cache.SetupSet(x => x.LastUpdate = It.Is<DateTime>(d => d > DateTime.UtcNow.AddMinutes(-1) &&
                                                                     d < DateTime.UtcNow.AddMinutes(1)));

            //Act
            var result = _sut.GetAllStats();

            //Assert
            _mockRepo.VerifyAll();
        }


    }
}