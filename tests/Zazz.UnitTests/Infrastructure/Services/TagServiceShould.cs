using System;
using System.Collections.Generic;
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
        private Mock<ITagStatsCache> _cache;
        private TagService _sut;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _staticRepo = _mockRepo.Create<IStaticDataRepository>();
            _cache = _mockRepo.Create<ITagStatsCache>();

            _sut = new TagService(_uow.Object, _staticRepo.Object, _cache.Object);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-2)]
        [TestCase(-3)]
        [TestCase(-4)]
        public void ReturnResultFromCacheIfItWasUpdatedLessThan5MinsAgo_OnGetTagStats(int minutesAgo)
        {
            //Arrange
            var list = new List<TagStat>();

            _cache.Setup(x => x.LastUpdate)
                  .Returns(DateTime.UtcNow.AddMinutes(minutesAgo));

            _cache.Setup(x => x.TagStats)
                  .Returns(list);

            //Act
            var result = _sut.GetAllTagStats();

            //Assert
            Assert.AreSame(list, result);
            _mockRepo.VerifyAll();
        }


    }
}