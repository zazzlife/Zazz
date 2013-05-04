using System;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class TagStatRepositoryShould
    {
        private TagStatRepository _repo;
        private ZazzDbContext _context;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new TagStatRepository(_context);
        }

        [Test]
        public void OrderTagsByDateAndReturnTheLatestOne_OnGetLastTag()
        {
            //Arrange
            byte tagId = 1;
            var tagStat1 = new TagStat
                           {
                               Date = DateTime.UtcNow.AddDays(-10),
                               TagId = tagId
                           };

            var tagStat2 = new TagStat
                           {
                               Date = DateTime.UtcNow.AddDays(-5),
                               TagId = tagId
                           };

            _context.TagStats.Add(tagStat1);
            _context.TagStats.Add(tagStat2);
            _context.SaveChanges();

            //Act
            var response = _repo.GetLastestTagStat(tagId);

            //Assert
            Assert.AreEqual(tagStat2.Id, response.Id);
        }
    }
}